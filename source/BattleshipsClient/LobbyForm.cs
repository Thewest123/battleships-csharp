using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BattleshipsLibrary;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.UDP;

namespace BattleshipsClient
{
    public partial class LobbyForm : Form
    {
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        public string ServerName { get; set; }
        public string PlayerName { get; set; }

        public LobbyForm(ConnectResponse response)
        {
            InitializeComponent();
            MinimumSize = MaximumSize = Size;

            //Pointery pro příchozí packety
            NetworkComms.AppendGlobalConnectionCloseHandler(ConnectionShutdownDelegate);
            NetworkComms.AppendGlobalIncomingPacketHandler<ChatMessage>("DisplayChatMessage", DisplayChatMessageDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("ChallengeAcceptRequest", ChallengeAcceptRequestDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("ChallengeFailed", ChallengeFailedDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<Client>("ChallengeAccepted", ChallengeAcceptedDelegatePointer);
            NetworkComms.AppendGlobalIncomingPacketHandler<List<Client>>("UpdateListInfo", UpdateListInfoDelegatePointer);

            //Nastavení dle odpovědi ze serveru
            lblServerIp.Text = ServerName = response.ServerName;
            lblPlayers.Text = response.ConnectedClients.Count.ToString("00");

            //Naplnění listu dle odpovědi ze serveru
            foreach (var client in response.ConnectedClients)
            {
                listPlayers.Items.Add(client.Name);
            }
        }

        #region Hlavní metody

        //Spojení se serverem ztraceno
        private void ConnectionShutdownDelegate(Connection connection)
        {
            MessageBox.Show($"Spojení se serverem {ServerName} ztraceno!");
            NetworkComms.Shutdown();
            Application.Exit();
        }

        //Aktualizace online klientů - packet pointer
        private void UpdateListInfoDelegatePointer(PacketHeader packetheader, Connection connection, List<Client> clients)
        {
            Invoke(new UpdateListDelegate(UpdateList), clients);
        }

        //Aktualizace online klientů - delegát
        private delegate void UpdateListDelegate(List<Client> clients);

        //Aktualizace online klientů
        private void UpdateList(List<Client> clients)
        {
            lblPlayers.Text = clients.Count.ToString("00");

            listPlayers.Items.Clear();
            foreach (var client in clients)
            {
                listPlayers.Items.Add(client.Name);
            }
        }

        //Výzva přijata
        private void ChallengeAcceptedDelegatePointer(PacketHeader packetheader, Connection connection, Client enemy)
        {
            Invoke(new OpenGameFormDelegate(OpenGameForm), ServerIp, ServerPort, enemy);
        }

        //Otevření okna se hrou - delegát
        private delegate void OpenGameFormDelegate(string serverIp, int serverPort, Client enemy);

        //Otevření okna se hrou
        private void OpenGameForm(string serverIp, int serverPort, Client enemy)
        {
            Hide();

            GameForm gameForm = new GameForm
            {
                ServerIp = serverIp,
                ServerPort = serverPort,
                EnemyIp = enemy.Ip,
                EnemyPort = enemy.Port,
                EnemyName = enemy.Name,
                PlayerName = PlayerName
            };

            gameForm.Show();
        }

        //Příchozí výzva ke hře
        private void ChallengeAcceptRequestDelegatePointer(PacketHeader packetheader, Connection connection, string name)
        {
            var response = MessageBox.Show($"Uživatel {name} Vás vyzval ke hře, přijmout?", "Výzva ke hře", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (response == DialogResult.Yes)
            {
                NetworkComms.SendObject("ChallengeAcceptInfo", ServerIp, ServerPort, true);
            }
            else
            {
                NetworkComms.SendObject("ChallengeAcceptInfo", ServerIp, ServerPort, false);
            }
        }

        //Nepodařilo se vyzvat hráče
        private void ChallengeFailedDelegatePointer(PacketHeader packetheader, Connection connection, string text)
        {
            MessageBox.Show(text, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //Příchozí zpráva do chatu - packet pointer
        private void DisplayChatMessageDelegatePointer(PacketHeader packetheader, Connection connection, ChatMessage message)
        {
            Invoke(new DisplayToChatDelegate(DisplayToChat), message);
        }

        //Příchozí zpráva do chatu - delegát
        private delegate void DisplayToChatDelegate(ChatMessage message);

        //Příchozí zpráva do chatu
        private void DisplayToChat(ChatMessage message)
        {
            rtbChat.Text += $"\n{message}";
        }

        //Výzva hráče
        private void Challenge()
        {
            string name = listPlayers.GetItemText(listPlayers.SelectedItem);

            if (name == PlayerName)
            {
                MessageBox.Show("Nemůžete vyzvat ke hře sami sebe!", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var response = MessageBox.Show($"Chcete vyzvat ke hře hráče {name}?", "Vyzvat ke hře",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (response == DialogResult.Yes)
                {
                    NetworkComms.SendObject("ChallengeRequest", ServerIp, ServerPort, name);
                }
            }
        }

        #endregion

        #region Form Eventy

        //Nastavení Accept Buttonu
        private void txtMessage_Click(object sender, EventArgs e)   => AcceptButton = btnSend;
        private void listPlayers_Click(object sender, EventArgs e)  => AcceptButton = btnChallenge;

        //Odeslání zprávy z chatu na server
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text != "")
            {
                ChatMessage message = new ChatMessage(PlayerName, txtMessage.Text);
                NetworkComms.SendObject("BroadcastChatMessage", ServerIp, ServerPort, message);
                txtMessage.Clear();
            }
        }


        //Výzva označeného hráče
        private void listPlayers_DoubleClick(object sender, EventArgs e)    => Challenge();
        private void btnChallenge_Click(object sender, EventArgs e)         => Challenge();

        //Zavření okna
        private void LobbyForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Ukončení procesu připojení
            NetworkComms.SendObject("Disconnect", ServerIp, ServerPort, "");
            NetworkComms.Shutdown();
            Application.Exit();
        }

        #endregion

        // Delegáti, protože nastávala chyba s procesy
        // http://programujte.com/forum/vlakno/9302-chyba-mezi-podprocesy/
    }
}
