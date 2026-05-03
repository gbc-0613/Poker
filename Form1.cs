using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hw2
{
    public partial class Form1 : Form
    {
        PictureBox[] pic = new PictureBox[5];
        int[] allPoker = new int[52];
        int[] playerPoker = new int[5];

        private GroupBox grpPorker;
        private GroupBox grpBet;
        private GroupBox grpButton;
        private Button btnDealCard;
        private Button btnChangeCard;
        private Button btnCheck;
        private TextBox txtResult;

        private TextBox txtMoney;
        private TextBox txtBet;
        private Button btnBet;
        private Button btnStop;
        private Label lblPayout;
        private Label lblWarning;

        private int totalMoney = 1000000;
        private int initialMoney = 1000000;
        private int currentBet = 0;
        private bool hasPlacedFirstBet = false;

        private readonly Dictionary<string, int> odds = new Dictionary<string, int>
        {
            { "同花大順", 250 },
            { "同花順", 50 },
            { "鐵支", 25 },
            { "葫蘆", 9 },
            { "同花", 6 },
            { "順子", 4 },
            { "三條", 3 },
            { "兩對", 2 },
            { "一對", 1 },
            { "雜牌", 0 }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeFormUi();
            InitializePoker();
        }

        private void InitializeFormUi()
        {
            Text = "五張撲克牌";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(800, 500);
            KeyPreview = true;
            KeyPress += frmPoker_KeyPress;

            grpPorker = new GroupBox { Name = "grpPorker", Text = "牌桌", Left = 20, Top = 20, Width = 750, Height = 220 };

            grpBet = new GroupBox { Name = "grpBet", Text = "下注", Left = 20, Top = 250, Width = 750, Height = 90 };
            Label lblMoneyTitle = new Label { Text = "總資金", Left = 20, Top = 38, AutoSize = true };
            txtMoney = new TextBox
            {
                Left = 95,
                Top = 34,
                Width = 160,
                Text = totalMoney.ToString(),
                ReadOnly = false,
                TextAlign = HorizontalAlignment.Right
            };
            Label lblBetTitle = new Label { Text = "押注金額", Left = 275, Top = 38, AutoSize = true };
            txtBet = new TextBox { Left = 350, Top = 34, Width = 90, Text = "500", TextAlign = HorizontalAlignment.Right };
            btnBet = new Button { Text = "押注", Left = 465, Top = 32, Width = 90 };
            btnStop = new Button { Text = "停止", Left = 570, Top = 32, Width = 90, Enabled = false };
            btnBet.Click += btnBet_Click;
            btnStop.Click += btnStop_Click;
            grpBet.Controls.Add(lblMoneyTitle);
            grpBet.Controls.Add(txtMoney);
            grpBet.Controls.Add(lblBetTitle);
            grpBet.Controls.Add(txtBet);
            grpBet.Controls.Add(btnBet);
            grpBet.Controls.Add(btnStop);

            grpButton = new GroupBox { Name = "grpButton", Text = "功能", Left = 20, Top = 350, Width = 750, Height = 120 };
            btnDealCard = new Button { Name = "btnDealCard", Text = "發牌", Left = 20, Top = 32, Width = 90, Enabled = false };
            btnDealCard.Click += btnDealCard_Click;
            btnChangeCard = new Button { Name = "btnChangeCard", Text = "換牌", Left = 120, Top = 32, Width = 90, Enabled = false };
            btnChangeCard.Click += btnChangeCard_Click;
            btnCheck = new Button { Name = "btnCheck", Text = "判斷牌型", Left = 220, Top = 32, Width = 100, Enabled = false };
            btnCheck.Click += btnCheck_Click;
            txtResult = new TextBox { Left = 340, Top = 32, Width = 360, ReadOnly = true };
            lblPayout = new Label { Left = 20, Top = 78, Width = 680, Height = 20, Text = "中獎金額: 0" };

            grpButton.Controls.Add(btnDealCard);
            grpButton.Controls.Add(btnChangeCard);
            grpButton.Controls.Add(btnCheck);
            grpButton.Controls.Add(txtResult);
            grpButton.Controls.Add(lblPayout);

            lblWarning = new Label
            {
                Left = 20,
                Top = 475,
                Width = 300,
                Height = 20,
                Text = "小賭怡情，大賭移家"
            };

            Controls.Add(grpPorker);
            Controls.Add(grpBet);
            Controls.Add(grpButton);
            Controls.Add(lblWarning);
        }

        private void InitializePoker()
        {
            for (int i = 0; i < 5; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Image = GetImage("back");
                pic[i].Name = "pic" + i;
                pic[i].SizeMode = PictureBoxSizeMode.AutoSize;
                pic[i].Top = 30;
                pic[i].Left = 10 + ((pic[i].Width + 10) * i);
                pic[i].Visible = true;
                pic[i].Enabled = false;
                pic[i].Tag = "back";
                grpPorker.Controls.Add(pic[i]);
                pic[i].MouseClick += new MouseEventHandler(pic_Click);
            }
        }

        private Image GetImage(string name)
        {
            return Properties.Resources.ResourceManager.GetObject(name) as Image;
        }

        private Image GetImage(int num)
        {
            return GetImage($"pic{num}");
        }

        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < allPoker.Length; i++)
            {
                int r = rand.Next(allPoker.Length);
                int temp = allPoker[r];
                allPoker[r] = allPoker[0];
                allPoker[0] = temp;
            }
        }

        private void btnBet_Click(object sender, EventArgs e)
        {
            if (!hasPlacedFirstBet)
            {
                if (!int.TryParse(txtMoney.Text, out int moneyInput) || moneyInput < 0)
                {
                    MessageBox.Show("總資金必須為 0 以上整數");
                    return;
                }
                totalMoney = moneyInput;
                initialMoney = moneyInput;
            }

            if (!int.TryParse(txtBet.Text, out int bet) || bet <= 0)
            {
                MessageBox.Show("押注金額必須為正整數");
                return;
            }
            if (bet > totalMoney)
            {
                int lack = bet - totalMoney;
                MessageBox.Show($"總資金不足，尚差 {lack:N0} 元");
                return;
            }

            currentBet = bet;
            totalMoney -= bet;
            txtMoney.Text = totalMoney.ToString();
            txtMoney.ReadOnly = true;
            hasPlacedFirstBet = true;
            btnBet.Enabled = false;
            txtBet.Enabled = false;
            btnDealCard.Enabled = true;
            txtResult.Text = "已押注，請發牌";
            lblPayout.Text = "中獎金額: 0";
        }

        private async void btnDealCard_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++) pic[i].Image = GetImage("back");
            await Task.Delay(500);
            for (int i = 0; i < 52; i++) allPoker[i] = i;
            Shuffle();
            for (int i = 0; i < 5; i++)
            {
                pic[i].Image = GetImage("pic" + (allPoker[i] + 1));
                playerPoker[i] = allPoker[i];
            }
            for (int i = 0; i < 5; i++)
            {
                pic[i].Enabled = true;
                pic[i].Tag = "front";
            }

            btnChangeCard.Enabled = true;
            btnCheck.Enabled = false;
            btnDealCard.Enabled = false;
            txtResult.Text = "";
        }

        private void pic_Click(object sender, MouseEventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            int index = int.Parse(p.Name.Replace("pic", ""));
            if (p.Tag.ToString() == "back")
            {
                p.Tag = "front";
                p.Image = GetImage(playerPoker[index] + 1);
            }
            else
            {
                p.Tag = "back";
                p.Image = GetImage("back");
            }
        }

        private void btnChangeCard_Click(object sender, EventArgs e)
        {
            int cardIndex = 5;
            for (int i = 0; i < pic.Length; i++)
            {
                if (pic[i].Tag.ToString() == "back")
                {
                    playerPoker[i] = allPoker[cardIndex];
                    pic[i].Image = GetImage(playerPoker[i] + 1);
                    pic[i].Tag = "front";
                    cardIndex++;
                }
            }
            for (int i = 0; i < pic.Length; i++) pic[i].Enabled = false;
            btnChangeCard.Enabled = false;
            btnCheck.Enabled = true;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string[] colorList = { "梅花", "方塊", "愛心", "黑桃" };
            string[] pointList = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            int[] pokerColor = new int[5];
            int[] pokerPoint = new int[5];
            for (int i = 0; i < 5; i++)
            {
                pokerColor[i] = playerPoker[i] % 4;
                pokerPoint[i] = playerPoker[i] / 4;
            }

            int[] colorCount = new int[4];
            int[] pointCount = new int[13];
            for (int i = 0; i < 5; i++)
            {
                int color = pokerColor[i];
                int point = pokerPoint[i];
                colorCount[color]++;
                pointCount[point]++;
            }

            Array.Sort(colorCount, colorList);
            Array.Reverse(colorCount);
            Array.Reverse(colorList);
            Array.Sort(pointCount, pointList);
            Array.Reverse(pointCount);
            Array.Reverse(pointList);

            bool isFlush = (colorCount[0] == 5);
            bool isSingle = (pointCount[0] == 1 && pointCount[1] == 1 && pointCount[2] == 1 && pointCount[3] == 1 && pointCount[4] == 1);
            bool isDiffFout = (pokerPoint.Max() - pokerPoint.Min() == 4);
            bool isRoyal = pokerPoint.Contains(0) && pokerPoint.Contains(9) && pokerPoint.Contains(10) && pokerPoint.Contains(11) && pokerPoint.Contains(12);
            bool isRoyalisFlush = isFlush && isRoyal;
            bool isStraightFlush = isFlush && isSingle && isDiffFout;
            bool isStraight = isSingle && (isDiffFout || isRoyal);
            bool isFourOfAKind = (pointCount[0] == 4);
            bool isFullHouse = (pointCount[0] == 3 && pointCount[1] == 2);
            bool isThreeOfAKind = (pointCount[0] == 3 && pointCount[1] == 1);
            bool isTwoPair = (pointCount[0] == 2 && pointCount[1] == 2);
            bool isOnePair = (pointCount[0] == 2 && pointCount[1] == 1);

            string result;
            if (isRoyalisFlush) result = $"{colorList[0]} 同花大順";
            else if (isStraightFlush) result = $"{colorList[0]} 同花順";
            else if (isStraight) result = "順子";
            else if (isFourOfAKind) result = $"{pointList[0]} 鐵支";
            else if (isFullHouse) result = $"{pointList[0]}三張{pointList[1]}兩張 葫蘆";
            else if (isFlush) result = $"{colorList[0]} 同花";
            else if (isThreeOfAKind) result = $"{pointList[0]} 三條";
            else if (isTwoPair) result = $"{pointList[0]},{pointList[1]} 兩對";
            else if (isOnePair) result = $"{pointList[0]} 一對";
            else result = "雜牌";

            txtResult.Text = result;

            int rate = 0;
            foreach (var k in odds.Keys)
            {
                if (result.Contains(k)) { rate = odds[k]; break; }
            }
            int payout = currentBet * rate;
            totalMoney += payout;
            txtMoney.Text = totalMoney.ToString();
            lblPayout.Text = $"中獎金額: {payout:N0} (賠率 {rate})";

            btnChangeCard.Enabled = false;
            btnCheck.Enabled = false;
            btnDealCard.Enabled = false;
            btnBet.Enabled = true;
            txtBet.Enabled = true;
            txtMoney.ReadOnly = true;
            btnStop.Enabled = true;
            currentBet = 0;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            int net = totalMoney - initialMoney;
            if (net >= 0)
            {
                MessageBox.Show($"共贏得 {net:N0} 元");
            }
            else
            {
                MessageBox.Show($"共輸了 {Math.Abs(net):N0} 元");
            }
            Application.Exit();
        }

        private void ShowCards()
        {
            for (int i = 0; i < 5; i++)
            {
                pic[i].Image = GetImage($"pic{playerPoker[i] + 1}");
            }
        }

        private void frmPoker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (btnDealCard.Enabled == false && btnBet.Enabled == false)
            {
                switch (e.KeyChar)
                {
                    case 'q': playerPoker[0] = 51; playerPoker[1] = 47; playerPoker[2] = 43; playerPoker[3] = 39; playerPoker[4] = 3; break;
                    case 'w': playerPoker[0] = 37; playerPoker[1] = 33; playerPoker[2] = 29; playerPoker[3] = 25; playerPoker[4] = 21; break;
                    case 'e': playerPoker[0] = 50; playerPoker[1] = 38; playerPoker[2] = 34; playerPoker[3] = 22; playerPoker[4] = 18; break;
                    case 'r': playerPoker[0] = 48; playerPoker[1] = 39; playerPoker[2] = 38; playerPoker[3] = 37; playerPoker[4] = 36; break;
                    case 't': playerPoker[0] = 30; playerPoker[1] = 29; playerPoker[2] = 6; playerPoker[3] = 5; playerPoker[4] = 4; break;
                    case 'y': playerPoker[0] = 48; playerPoker[1] = 39; playerPoker[2] = 15; playerPoker[3] = 14; playerPoker[4] = 13; break;
                    default: return;
                }
                ShowCards();
            }
        }
    }
}
