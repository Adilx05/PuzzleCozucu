using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Puzzle;

namespace PuzzleCoz2
{
    public partial class Form1 : Form
    {
        private MantikPuzzle _m;
        private Mantiksal _mMantiksal;
        private LinearKaristir<int> _mKaristir;
        private WindowsFormsSynchronizationContext mSyncContext;
        Dictionary<int, Button> _mButtons;
        private int[] _mInitialState;
        private bool _mBusy;

        public Form1()
        {
            InitializeComponent();
            mSyncContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;

            Initialize();
        }

        private void Initialize()
        {
            _mInitialState = new int[] { 8, 7, 2, 4, 6, 3, 1, -1, 5 };

            _mKaristir = new LinearKaristir<int>();
            _m = new MantikPuzzle();
            _mMantiksal = Mantiksal.SiracilOyun;
            _m.KonumDegismesi += KonumDegismesi;
            _m.OnCozum += OnCozum;

            // Set display nodes
            _mButtons = new Dictionary<int, Button>();
            _mButtons[0] = button1;
            _mButtons[1] = button2;
            _mButtons[2] = button3;
            _mButtons[3] = button4;
            _mButtons[4] = button5;
            _mButtons[5] = button6;
            _mButtons[6] = button7;
            _mButtons[7] = button8;
            _mButtons[8] = button9;

            // Display state
            DisplayState(_mInitialState, false);

            statusLabel.Text = "Çözek Bakam";
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;
            merkezuzMenu.Checked = true;
        }

        private void DegerDegistir(int x, int y)
        {
            int temp = _mInitialState[x];
            _mInitialState[x] = _mInitialState[y];
            _mInitialState[y] = temp;
        }

        private void KonumDegismesi(int[] state, bool isFinal)
        {
            mSyncContext.Post(item => DisplayState(state, isFinal), null);
            Thread.Sleep(1500);
        }

        private void OnCozum(int steps, int time, int statesExamined)
        {
            Action action = () =>
            {
                progressBar.Visible = false;
                this.Cursor = Cursors.Default;

                if (steps > -1)
                {
                    statusLabel.Text = "Adım: " + steps.ToString("n0") + ", Zaman: " + (time / 1000.0).ToString("n2") + ", Varyasyon: " + statesExamined.ToString("n0");
                    MessageBox.Show(this, "Çözüm Bulundu !", "Bilgi !");
                }
                else
                {
                    statusLabel.Text = "Adım: Yok, Zaman: " + (time / 1000.0).ToString("n3") + "Saniye, Varyasyon: " + statesExamined.ToString("n0");
                    MessageBox.Show(this, "Çözüm Bulunamadı", "Bilgi !");
                }
            };

            mSyncContext.Send(item => action.Invoke(), null);
        }

        private void DisplayState(int[] nodes, bool isFinal)
        {
            if (nodes != null)
            {
                this.gamePanel.SuspendLayout();

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] > 0)
                    {
                        _mButtons[i].Text = nodes[i].ToString();
                    }
                    else
                    {
                        _mButtons[i].Text = null;
                    }
                }

                this.gamePanel.ResumeLayout();
            }

            if (isFinal)
            {
                _mBusy = false;
                buttonShuffle.Enabled = true;
                buttonStart.Enabled = true;
            }
        }

        private void CozmeyeBasla()
        {
            _m.Coz(_mInitialState, _mMantiksal);

            progressBar.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            statusLabel.Text = "Çözüm Bulunuyor...";
            _mBusy = true;
        }

        private bool IslemIzni()
        {
            return !_mBusy;
        }

        private void Karistir_Button_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                _mKaristir.Shuffle(_mInitialState);
                DisplayState(_mInitialState, false);
            }
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (IslemIzni())
            {
                Button button = sender as Button;

                if (button != null && button.Tag != null)
                {
                    int deger;
                    Button tileButton;

                    if (int.TryParse(button.Tag.ToString(), out deger) && _mButtons.TryGetValue(deger, out tileButton) && button == tileButton)
                    {
                        button.DoDragDrop(button.Tag, DragDropEffects.Copy | DragDropEffects.Move);
                    }
                }
            }
        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            if (IslemIzni())
            {
                if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void Button_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (IslemIzni())
            {
                Button button = sender as Button;
                if (button != null && button.Tag != null)
                {
                    int dropValue;
                    Button buttonToDrop;

                    if (int.TryParse(button.Tag.ToString(), out dropValue) && _mButtons.TryGetValue(dropValue, out buttonToDrop) && button == buttonToDrop)
                    {
                        int dragValue;

                        if (int.TryParse(e.Data.GetData(DataFormats.Text).ToString(), out dragValue) && dropValue != dragValue)
                        {
                            DegerDegistir(dragValue, dropValue);
                            DisplayState(_mInitialState, false);
                        }
                    }
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                CozmeyeBasla();
            }
        }

        private void Karistir_Menu_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                _mKaristir.Shuffle(_mInitialState);
                // Display state
                DisplayState(_mInitialState, false);
            }
        }

        private void Coz_Menu_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                CozmeyeBasla();
            }
        }

        private void Cikis_Menu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Merkez_Menu_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                _mMantiksal = Mantiksal.SiracilOyun;
                merkezuzMenu.Checked = true;
                boslukitemMenu.Checked = false;
            }
        }

        private void Bosluk_Menu_Click(object sender, EventArgs e)
        {
            if (IslemIzni())
            {
                _mMantiksal = Mantiksal.BoslugaOyna;
                boslukitemMenu.Checked = true;
                merkezuzMenu.Checked = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Adil Tarafından Patatesli Poğaça Takımı İçin Hazırlandı :)", "Hakkında");
        }
    }

}
