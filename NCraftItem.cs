using _menu = AAMenu.Menu;
using ModKit.Helper;
using ModKit.Interfaces;
using Life;
using Life.Network;
using NCraft.Panels;
using Mirror;
using Life.DB;
using Life.CheckpointSystem;
using UnityEngine;
using NCraft.Entities;

namespace Main
{
    public class NCraft : ModKit.ModKit
    {
        private PanelsAdmin PanelsAdmin;
        private PanelCraft PanelCraft;
        public NCraft(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.1", "Noah FOURNIER");
            PanelCraft = new PanelCraft(this);
            PanelsAdmin = new PanelsAdmin(this, PanelCraft);
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            Orm.RegisterTable<PointsCraft>();
            InsertMenu();

            new SChatCommand("/ncraft".ToLower(), "Configuration du plugin NCraft", "/NCraft", (player, args) =>
            {
                if (player.account.AdminLevel >= 5)
                {
                    PanelsAdmin.PanelPrincipal(player);
                }
            }).Register();

            ModKit.Internal.Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
        }

        public void InsertMenu()
        {
            _menu.AddAdminPluginTabLine(PluginInformations, 5, "NCraft - Configuration", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                PanelsAdmin.PanelPrincipal(player);
            }, 0);
        }

        public override async void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);

            var points = await PointsCraft.QueryAll();

            foreach (var point in points)
            {
                NCheckpoint checkpoint = new NCheckpoint(player.netId, new Vector3(point.Pos_X, point.Pos_Y, point.Pos_Z), ui =>
                {
                    PanelCraft.Craft(player, point);
                });

                player.CreateCheckpoint(checkpoint);
            }
        }
    }
}