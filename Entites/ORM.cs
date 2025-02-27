using System.Collections.Generic;
using SQLite;
using ModKit.Helper.CraftHelper;
using Life.Network;
using Newtonsoft.Json;

namespace NCraft.Entities
{
    public class PointsCraft : ModKit.ORM.ModEntity<PointsCraft>
    {
        [AutoIncrement][PrimaryKey] public int Id { get; set; }

        public string Nom { get; set; }
        public string RecetteJson { get; set; }
        public int Temps { get; set; }
        public int Prix { get; set; }
        public int JoueursSimultanesMax { get; set; }
        public bool AfficherListe { get; set; }
        public float Pos_X { get; set; }
        public float Pos_Y { get; set; }
        public float Pos_Z { get; set; }

        [Ignore] public List<Player> JoueursActuels { get; set; } = new List<Player>();

        [Ignore]
        public Recipe Recette
        {
            get
            {
                return string.IsNullOrEmpty(RecetteJson) ? null : JsonConvert.DeserializeObject<Recipe>(RecetteJson);
            }
            set
            {
                RecetteJson = JsonConvert.SerializeObject(value);
            }
        }

        public PointsCraft() { }
    }
}
