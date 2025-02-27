using Life.Network;
using ModKit.Helper;
using NCraft.Utils;
using SQLite;
using ModKit.Utils;
using ModKit.Helper.CraftHelper;
using System.Collections.Generic;
using NCraft.Entities;
using Life;
using Life.CheckpointSystem;
using UnityEngine;

namespace NCraft.Panels
{
    internal class PanelsAdmin
    {
        [Ignore] public ModKit.ModKit Context { get; set; }
        private PanelCraft PanelCraft;

        public PanelsAdmin(ModKit.ModKit context, PanelCraft panelCraft)
        {
            Context = context;
            PanelCraft = panelCraft;
        }

        public void PanelPrincipal(Player player)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("NCraft - <i>Par noah_fournier</i>"), Life.UI.UIPanel.PanelType.Text, player, () => PanelPrincipal(player));

            panel.TextLines.Add("Que souhaitez-vous configurer ?");

            panel.NextButton(Couleurs.Bleu("RECETTES"), () => PanelRecettes(player));
            panel.NextButton(Couleurs.Bleu("POINTS"), () => PanelPoints(player));
            panel.CloseButton(Couleurs.Rouge("FERMER"));

            panel.Display();
        }

        public async void PanelRecettes(Player player)
        {
            int select = 0;

            var recettes = await Context.CraftHelper.GetRecipes();

            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Gestion des recettes"), Life.UI.UIPanel.PanelType.TabPrice, player, () => PanelRecettes(player));

            foreach (var recette in recettes)
            {
                panel.AddTabLine(recette.Name, recette.Category, Icones.RecupererIcone(recette.ObjectId), ui =>
                {
                    if (select == 1)
                    {
                        PanelIngredients(player, recette);
                    }
                    else if (select == 2)
                    {
                        SupprimerRecette(player, recette, panel);
                    }
                });
            }

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Rose("INGRÉDIENTS"), () =>
            {
                select = 1;
                panel.SelectTab();
            });
            panel.AddButton(Couleurs.Rouge("SUPPRIMER"), ui2 =>
            {
                select = 2;
                panel.SelectTab();
            });
            panel.NextButton(Couleurs.Vert("AJOUTER"), () => PanelAjouterRecette(player));

            panel.Display();
        }

        public void PanelIngredients(Player player, Recipe recette)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Gestion des ingrédients"), Life.UI.UIPanel.PanelType.TabPrice, player, () => PanelIngredients(player, recette));

            if (recette.LIngredientList != null)
            {
                foreach (var ingredient in recette.LIngredientList)
                {
                    panel.AddTabLine($"{ItemUtils.GetItemById(ingredient.ItemId)?.itemName}", $"{ingredient.Count}", Icones.RecupererIcone(ingredient.ItemId), _ => { });
                }
            }
            else panel.AddTabLine($"Aucun ingrédient", _ => { });

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));

            panel.Display();
        }

        public async void SupprimerRecette(Player player, Recipe recette, Panel panel)
        {
            await recette.Delete();
            player.SendText(Couleurs.Vert($"La recette {recette.Name} a bien été supprimée !"));
            panel.Refresh();
        }

        public void PanelAjouterRecette(Player player)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Ajouter une recette"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterRecette(player));

            panel.TextLines.Add("Indiquez l'id de l'objet crafté");
            panel.SetInputPlaceholder("ID de l'objet crafté");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () =>
            {
                if (int.TryParse(panel.inputText, out int itemId) && ItemUtils.GetItemById(itemId) != null)
                {
                    PanelAjouterRecette2(player, itemId);
                }
                else player.Notify("Item incorrect", "L'item indiqué n'existe pas !", Life.NotificationManager.Type.Error);
            });

            panel.Display();
        }

        public void PanelAjouterRecette2(Player player, int itemId)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Ajouter une recette"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterRecette2(player, itemId));

            panel.TextLines.Add("Indiquez la catégorie de la recette");
            panel.SetInputPlaceholder("Catégorie de la recette");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () => PanelAjouterRecette3(player, itemId, panel.inputText));

            panel.Display();
        }

        public void PanelAjouterRecette3(Player player, int itemId, string categorie)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Ajouter une recette"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterRecette3(player, itemId, categorie));

            panel.TextLines.Add("Indiquez les ingrédients au format : id:quantité,id:quantité,...");
            panel.SetInputPlaceholder("Exemple : 1:2,5:3,10:1");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.AddButton(Couleurs.Vert("VALIDER"), async ui =>
            {
                player.ClosePanel(panel);

                List<Ingredient> ingredients = new List<Ingredient>();

                if (!string.IsNullOrEmpty(panel.inputText))
                {
                    string[] entrees = panel.inputText.Split(',');

                    foreach (string entree in entrees)
                    {
                        string[] parts = entree.Split(':');
                        if (parts.Length != 2 || !int.TryParse(parts[0], out int item) || !int.TryParse(parts[1], out int count) || count < 0)
                        {
                            player.Notify("Format invalide", "Utilisez : id:quantité,id:quantité,...", Life.NotificationManager.Type.Error);
                            return;
                        }

                        if (ItemUtils.GetItemById(item) == null)
                        {
                            player.Notify("Item invalide", $"L'item {item} n'existe pas.", Life.NotificationManager.Type.Error);
                            return;
                        }

                        ingredients.Add(new Ingredient { ItemId = item, Count = count });
                    }
                }
                else
                {
                    player.SendText(Couleurs.Rouge("Aucun ingrédient ajouté. La recette sera sans ingrédient."));
                }

                Recipe nouvelle_recette = new Recipe
                {
                    Name = ItemUtils.GetItemById(itemId).itemName,
                    ObjectId = itemId,
                    Category = categorie,
                    IsVehicle = false,
                    LIngredientList = ingredients
                };

                nouvelle_recette.SerializeIngredients();

                await Context.CraftHelper.AddRecipe(nouvelle_recette);

                PanelRecettes(player);

                player.SendText(Couleurs.Vert($"La recette {nouvelle_recette.Name} a été enregistrée !"));
            });


            panel.Display();
        }

        public async void PanelPoints(Player player)
        {
            var points = await PointsCraft.QueryAll();

            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Gestion des points"), Life.UI.UIPanel.PanelType.TabPrice, player, () => PanelPoints(player));

            foreach (var point in points)
            {
                var recette = point.Recette;
                panel.AddTabLine(point.Nom, recette.Name, Icones.RecupererIcone(recette.ObjectId), ui => SupprimerPoint(player, point));
            }

            panel.AddButton(Couleurs.Rouge("SUPPRIMER"), ui2 => panel.SelectTab());
            panel.NextButton(Couleurs.Vert("AJOUTER"), () => PanelAjouterPoint(player));
            panel.PreviousButton(Couleurs.Bleu("RETOUR"));

            panel.Display();
        }

        public async void SupprimerPoint(Player player, PointsCraft point)
        {
            await point.Delete();
            player.SendText(Couleurs.Vert($"Le point {point.Nom} a bien été supprimé !"));
            player.SendText(Couleurs.Orange("Le changement sera effectif après s'être déconnecté !"));
        }

        public void PanelAjouterPoint(Player player)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Ajouter un point"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterPoint(player));

            panel.TextLines.Add("Indiquez le nom du point");
            panel.SetInputPlaceholder("Nom du point");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () => PanelAjouterPoint2(player, panel.inputText));

            panel.Display();
        }

        public async void PanelAjouterPoint2(Player player, string nom)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Choisissez la recette associée"), Life.UI.UIPanel.PanelType.TabPrice, player, () => PanelAjouterPoint2(player, nom));

            var recettes = await Context.CraftHelper.GetRecipes();

            foreach (var recette in recettes)
            {
                panel.AddTabLine(recette.Name, recette.Category, Icones.RecupererIcone(recette.ObjectId), ui => PanelAjouterPoint3(player, nom, recette));
            }

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () => panel.SelectTab());

            panel.Display();
        }

        public void PanelAjouterPoint3(Player player, string nom, Recipe recette)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Ajouter un point"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterPoint3(player, nom, recette));

            panel.TextLines.Add("Indiquez le temps de craft");
            panel.SetInputPlaceholder("Temps de craft en secondes");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("SUIVANT"), () =>
            {
                if (int.TryParse(panel.inputText, out int temps) && temps > 0)
                {
                    PanelAjouterPoint4(player, nom, recette, temps);
                }
                else player.Notify("Temps incorrect", "Le temps doit être positif !", Life.NotificationManager.Type.Error);
            });

            panel.Display();
        }

        public void PanelAjouterPoint4(Player player, string nom, Recipe recette, int temps)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Configurer le point"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterPoint4(player, nom, recette, temps));

            panel.TextLines.Add("Indiquez le nombre maximum de joueurs pouvant utiliser ce point simultanément.");
            panel.SetInputPlaceholder("Exemple : 3");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("VALIDER"), () =>
            {
                if (int.TryParse(panel.inputText, out int joueursMax) && joueursMax > 0)
                {
                    PanelAjouterPoint5(player, nom, recette, temps, joueursMax);
                }
                else
                {
                    player.Notify("Valeur incorrecte", "Le nombre de joueurs doit être un entier positif !", Life.NotificationManager.Type.Error);
                }
            });

            panel.Display();
        }

        public void PanelAjouterPoint5(Player player, string nom, Recipe recette, int temps, int joueursMax)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Configurer le point"), Life.UI.UIPanel.PanelType.Text, player, () => PanelAjouterPoint5(player, nom, recette, temps, joueursMax));

            panel.TextLines.Add("Souhaitez-vous que la liste des ingrédients soit visible avant le craft ?");

            panel.NextButton(Couleurs.Vert("OUI"), () =>
            {
                PanelAjouterPoint6(player, nom, recette, temps, joueursMax, true);
            });
            panel.NextButton(Couleurs.Rouge("NON"), () =>
            {
                PanelAjouterPoint6(player, nom, recette, temps, joueursMax, false);
            });
            panel.PreviousButton(Couleurs.Bleu("RETOUR"));

            panel.Display();
        }

        public void PanelAjouterPoint6(Player player, string nom, Recipe recette, int temps, int joueursMax, bool afficherListe)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Configurer le point"), Life.UI.UIPanel.PanelType.Input, player, () => PanelAjouterPoint6(player, nom, recette, temps, joueursMax, afficherListe));

            panel.TextLines.Add("Indiquez le prix du craft (0 si gratuit) :");
            panel.SetInputPlaceholder("Prix du craft");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.NextButton(Couleurs.Vert("VALIDER"), () =>
            {
                if (int.TryParse(panel.inputText, out int prix) && prix >= 0)
                {
                    PanelAjouterPoint7(player, nom, recette, temps, joueursMax, afficherListe, prix);
                }
                else
                {
                    player.Notify("Prix invalide", "Le prix doit être un nombre entier !", Life.NotificationManager.Type.Error);
                }
            });

            panel.Display();
        }

        public void PanelAjouterPoint7(Player player, string nom, Recipe recette, int temps, int joueursMax, bool afficherListe, int prix)
        {
            Panel panel = Context.PanelHelper.Create(Couleurs.Bleu("Configurer le point"), Life.UI.UIPanel.PanelType.Text, player, () => PanelAjouterPoint7(player, nom, recette, temps, joueursMax, afficherListe, prix));

            panel.TextLines.Add($"Placez-vous où vous souhaitez ajouter le point et appuyez sur {Couleurs.Vert("\"CONFIRMER\"")}");

            panel.PreviousButton(Couleurs.Bleu("RETOUR"));
            panel.AddButton(Couleurs.Vert("AJOUTER"), async ui =>
            {
                player.ClosePanel(panel);

                PointsCraft point = new PointsCraft
                {
                    Nom = nom,
                    Recette = recette,
                    Temps = temps,
                    Prix = prix,
                    JoueursSimultanesMax = joueursMax,
                    AfficherListe = afficherListe,
                    Pos_X = player.setup.transform.position.x,
                    Pos_Y = player.setup.transform.position.y,
                    Pos_Z = player.setup.transform.position.z
                };

                await point.Save();

                foreach (Player joueur in Nova.server.GetAllInGamePlayers())
                {
                    NCheckpoint checkpoint = new NCheckpoint(player.netId, new Vector3(point.Pos_X, point.Pos_Y, point.Pos_Z), ui2 =>
                    {
                        PanelCraft.Craft(player, point);
                    });

                    joueur.CreateCheckpoint(checkpoint);
                }

                PanelPoints(player);

                player.SendText(Couleurs.Vert($"Le point {point.Nom} a bien été ajouté avec un prix de {prix} !"));
            });

            panel.Display();
        }

    }
}
