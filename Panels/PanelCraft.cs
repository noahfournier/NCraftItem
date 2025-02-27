using System.Collections;
using System.Collections.Generic;
using Life.Network;
using ModKit.Helper;
using ModKit.Helper.CraftHelper;
using ModKit.Utils;
using NCraft.Entities;
using NCraft.Utils;
using SQLite;
using UnityEngine;

namespace NCraft.Panels
{
    internal class PanelCraft
    {
        [Ignore] public ModKit.ModKit Context { get; set; }

        public PanelCraft(ModKit.ModKit context)
        {
            Context = context;
        }

        public void Craft(Player player, PointsCraft point)
        {
            if (point.JoueursActuels.Count >= point.JoueursSimultanesMax)
            {
                player.Notify("Point occupé", "Trop de joueurs utilisent ce point actuellement.", Life.NotificationManager.Type.Error);
                return;
            }

            if (point.Prix > 0 && player.Money < point.Prix)
            {
                player.Notify("Fonds insuffisants", $"Vous avez besoin de {point.Prix}€ pour crafter cela !", Life.NotificationManager.Type.Error);
                return;
            }

            if (point.AfficherListe)
            {
                PanelIngredients(player, point.Recette, point);
            }
            else
            {
                if (point.Recette.LIngredientList.Count > 0 && !PossedeLesIngredients(player, point.Recette.LIngredientList))
                {
                    player.Notify("Ingrédients manquants", "Vous n'avez pas tous les ingrédients nécessaires !", Life.NotificationManager.Type.Error);
                    return;
                }

                if (point.Recette.LIngredientList.Count > 0)
                {
                    RetirerIngredients(player, point.Recette.LIngredientList);
                }

                if (point.Prix > 0)
                {
                    player.AddMoney(-point.Prix, "POINT_CRAFT");
                    player.Notify("Paiement effectué", $"Vous avez payé {point.Prix}$ pour crafter.", Life.NotificationManager.Type.Success);
                }

                point.JoueursActuels.Add(player);
                player.SendText(Couleurs.Bleu($"Ne bougez pas pendant {point.Temps} secondes pour terminer le craft !"));

                player.setup.StartCoroutine(VerifierImmobilite(player, point));
            }
        }

        public void PanelIngredients(Player player, Recipe recette, PointsCraft point)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Liste des ingrédients"), Life.UI.UIPanel.PanelType.TabPrice, player, () => PanelIngredients(player, recette, point));

            if (recette.LIngredientList != null && recette.LIngredientList.Count > 0)
            {
                foreach (var ingredient in recette.LIngredientList)
                {
                    panel.AddTabLine($"{ItemUtils.GetItemById(ingredient.ItemId)?.itemName}", $"Quantité : {ingredient.Count}", Icones.RecupererIcone(ingredient.ItemId), _ => { });
                }
            }
            else
            {
                panel.AddTabLine($"Aucun ingrédient nécessaire", _ => { });
            }

            panel.AddTabLine(Couleurs.Orange($"Prix du craft : {point.Prix}€"), "", Icones.RecupererIcone(1322), _ => { });

            panel.PreviousButton(Couleurs.Rouge("FERMER"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () =>
            {
                if (point.Prix > 0 && player.Money < point.Prix)
                {
                    player.Notify("Fonds insuffisants", $"Vous avez besoin de {point.Prix}€ en poche pour crafter ceci !", Life.NotificationManager.Type.Error);
                    return;
                }

                if (recette.LIngredientList.Count > 0 && !PossedeLesIngredients(player, recette.LIngredientList))
                {
                    player.Notify("Ingrédients manquants", "Vous n'avez pas tous les ingrédients nécessaires !", Life.NotificationManager.Type.Error);
                }
                else
                {
                    player.ClosePanel(panel);

                    if (recette.LIngredientList.Count > 0)
                    {
                        RetirerIngredients(player, recette.LIngredientList);
                    }

                    if (point.Prix > 0)
                    {
                        player.AddMoney(-point.Prix, "POINT_CRAFT");
                        player.Notify("Paiement effectué", $"Vous avez payé {point.Prix}$ pour crafter.", Life.NotificationManager.Type.Success);
                    }

                    point.JoueursActuels.Add(player);
                    player.SendText(Couleurs.Bleu($"Ne bougez pas pendant {point.Temps} secondes pour terminer le craft !"));

                    player.setup.StartCoroutine(VerifierImmobilite(player, point));
                }
            });

            panel.Display();
        }

        private IEnumerator VerifierImmobilite(Player player, PointsCraft point)
        {
            float tempsRestant = point.Temps;

            yield return new WaitForSeconds(1);
            Vector3 positionInitiale = player.setup.transform.position;

            while (tempsRestant > 0)
            {
                yield return new WaitForSeconds(1);

                if (Vector3.Distance(player.setup.transform.position, positionInitiale) > 0.1f)
                {
                    player.Notify("Échec", "Vous avez bougé ! Craft annulé.", Life.NotificationManager.Type.Error);
                    point.JoueursActuels.Remove(player);
                    yield break;
                }

                tempsRestant--;
            }

            AjouterObjetAuJoueur(player, point.Recette.ObjectId);
            player.SendText(Couleurs.Vert($"Félicitations ! Vous avez crafté {point.Recette.Name}."));
            point.JoueursActuels.Remove(player);
        }

        private bool PossedeLesIngredients(Player player, List<Ingredient> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                if (!InventoryUtils.CheckInventoryContainsItem(player, ingredient.ItemId, ingredient.Count))
                    return false;
            }
            return true;
        }

        private void RetirerIngredients(Player player, List<Ingredient> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                InventoryUtils.RemoveFromInventory(player, ingredient.ItemId, ingredient.Count);
            }
        }

        private void AjouterObjetAuJoueur(Player player, int objetId)
        {
            InventoryUtils.AddItem(player, objetId, 1);
        }
    }
}
