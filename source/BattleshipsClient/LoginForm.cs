using System;
using System.Windows.Forms;
using BattleshipsLibrary;
using NetworkCommsDotNet;

namespace BattleshipsClient
{
    public partial class LoginForm : Form
    {
        public static LobbyForm Lobby;
        
        public LoginForm()
        {
            InitializeComponent();
            MinimumSize = MaximumSize = Size;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //Odeslání žádosti o připojení a vyčkání 10 sekund na odpověď serveru
                var response = NetworkComms.SendReceiveObject<string, ConnectResponse>("ConnectUser", txtIp.Text, (int) numPort.Value, "ConnectInfo", 10000, txtName.Text);

                //Žádost přijata
                if (response.ResponseType == ResponseType.Accepted)
                {
                    //Skrytí Login formu
                    Hide();

                    //Otevření Lobby formu
                    Lobby = new LobbyForm(response)
                    {
                        ServerIp = txtIp.Text,
                        ServerPort = (int) numPort.Value,
                        PlayerName = txtName.Text
                    };
                    Lobby.Show();
                }
                //Žádost zamítnuta
                else
                {
                    //Zobrazení důvodu zamítnutí
                    MessageBox.Show(string.Format($"Server {response.ServerName} odmítl spojení se zprávou: {response.Response}"));
                }
            }
            catch
            {
                //Připojení se nezdařilo
                MessageBox.Show("K severu není možné se připojit, zkontrolujte připojení a správnost zadaných údajů");
            }
        }
    }
}
