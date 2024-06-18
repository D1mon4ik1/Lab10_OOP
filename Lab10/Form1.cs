using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Lab10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            thread1 = new Thread(new ThreadStart(CastEncryption));
            thread2 = new Thread(new ThreadStart(Md4Hashing));
            thread3 = new Thread(new ThreadStart(SealEncryption));
        }
        
        Thread thread1;
        Thread thread2;
        Thread thread3;
        
        private volatile bool thread1Running = false;
        private volatile bool thread2Running = false;
        private volatile bool thread3Running = false;
        
        private void CastEncryption()
        {
            try
            {
                var engine = new Cast5Engine();
                var key = new byte[16]; // 128-bit key
                new SecureRandom().NextBytes(key);
                var keyParam = new KeyParameter(key);

                var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
                cipher.Init(true, new ParametersWithIV(keyParam, new byte[8]));

                var plaintext = Encoding.UTF8.GetBytes("This is a test message");
                var ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
                var length = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
                cipher.DoFinal(ciphertext, length);

                richTextBox1.Invoke((MethodInvoker)delegate ()
                {
                    richTextBox1.Text = BitConverter.ToString(ciphertext).Replace("-", "");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                thread1Running = false;
            }
        }

        private void Md4Hashing()
        {
            try
            {
                var digest = new MD4Digest();
                var input = Encoding.UTF8.GetBytes("This is a test message");
                digest.BlockUpdate(input, 0, input.Length);

                var output = new byte[digest.GetDigestSize()];
                digest.DoFinal(output, 0);

                richTextBox2.Invoke((MethodInvoker)delegate ()
                {
                    richTextBox2.Text = BitConverter.ToString(output).Replace("-", "");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                thread2Running = false;
            }
        }

        private void SealEncryption()
        {
            try
            {
                var engine = new SealEngine();
                var key = new byte[16]; // 128-bit key
                new SecureRandom().NextBytes(key);
                var keyParam = new KeyParameter(key);

                engine.Init(true, keyParam);

                var plaintext = Encoding.UTF8.GetBytes("This is a test message");
                var ciphertext = new byte[plaintext.Length];
                engine.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);

                richTextBox3.Invoke((MethodInvoker)delegate ()
                {
                    richTextBox3.Text = BitConverter.ToString(ciphertext).Replace("-", "");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                thread3Running = false;
            }
        }

        public class SealEngine
        {
            private uint[] S = new uint[512];
            private uint[] X = new uint[4];
            private int counter = 0;

            public void Init(bool forEncryption, ICipherParameters parameters)
            {
                KeyParameter keyParam = (KeyParameter)parameters;
                byte[] key = keyParam.GetKey();
                InitializeS(key);
                InitializeX(key);
            }

            public void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
            {
                for (int i = 0; i < length; i++)
                {
                    if (counter == 0)
                    {
                        GenerateNextBlock();
                    }
                    output[outOff + i] = (byte)(input[inOff + i] ^ S[counter]);
                    counter = (counter + 1) & 0x1FF;
                }
            }

            private void InitializeS(byte[] key)
            {
                for (int i = 0; i < 256; i++)
                {
                    S[i] = BitConverter.ToUInt32(key, (i * 4) % key.Length);
                }
                for (int i = 256; i < 512; i++)
                {
                    S[i] = S[i - 256];
                }
            }

            private void InitializeX(byte[] key)
            {
                for (int i = 0; i < 4; i++)
                {
                    X[i] = BitConverter.ToUInt32(key, (i * 4) % key.Length);
                }
            }

            private void GenerateNextBlock()
            {
                for (int i = 0; i < 4; i++)
                {
                    X[i] += S[(X[(i + 1) & 3] >> 10) & 0x1FF];
                    S[(X[i] >> 10) & 0x1FF] = X[i];
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!thread1Running)
                {
                    thread1 = new Thread(new ThreadStart(CastEncryption));
                    thread1.Start();
                    thread1Running = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!thread2Running)
                {
                    thread2 = new Thread(new ThreadStart(Md4Hashing));
                    thread2.Start();
                    thread2Running = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!thread3Running)
                {
                    thread3 = new Thread(new ThreadStart(SealEncryption));
                    thread3.Start();
                    thread3Running = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                thread1.Suspend();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                thread2.Suspend();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                thread3.Suspend();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (!thread1Running)
                {
                    thread1 = new Thread(new ThreadStart(CastEncryption));
                    thread1.Start();
                    thread1Running = true;
                }

                if (!thread2Running)
                {
                    thread2 = new Thread(new ThreadStart(Md4Hashing));
                    thread2.Start();
                    thread2Running = true;
                }

                if (!thread3Running)
                {
                    thread3 = new Thread(new ThreadStart(SealEncryption));
                    thread3.Start();
                    thread3Running = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                thread1.Suspend();
                thread2.Suspend();
                thread3.Suspend();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            thread1.Abort();
            thread2.Abort();
            thread3.Abort();
        }
    }
}
