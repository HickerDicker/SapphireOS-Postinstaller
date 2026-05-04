using SapphireOS_PostInstall.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using SapphireOS_PostInstall.Dialogs;

namespace SapphireOS_PostInstall
{
    public partial class EndScreen : UserControl
    {
        private System.Windows.Forms.Timer countdownTimer = new System.Windows.Forms.Timer();

        private static EndScreen _instance;
        public static EndScreen Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EndScreen();
                return _instance;
            }
        }
        public EndScreen()
        {
            InitializeComponent();
            StartCountdown();
        }
        private int _countdown = 60;

        private void StartCountdown()
        {
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += countdownTimerTick;
            countdownTimer.Start();
        }

        private void countdownTimerTick(object sender, EventArgs e)
        {
            label1.Text = _countdown.ToString();
            if (_countdown <= 0)
            {
                countdownTimer.Stop();
                Utils.RunCommandSilent("shutdown.exe", "-r -t 00");
                return;
            }
            _countdown--;
        }

        private void EndScreen_Load(object sender, EventArgs e)
        {
            // idk if there is any better way to do this in a way where the form doesn't freeze if you have any suggestions lmk cause I feel like this is very stupid but if it works it works dont touch it
            var t = new Thread(() =>
            {
                Application.Run(new EndDialog());
            });
            t.IsBackground = true;
            t.Start();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Utils.RunCommandSilent("shutdown.exe", "-r -t 00");
        }
    }
}
