using System;
using System.Collections.Generic;
using Life;

namespace NCraft.Utils
{
    public static class Icones
    {
        private static Dictionary<int, int> Exceptions = new Dictionary<int, int>
        {
            //{ iconeIdRemplacé, IconIdRemplacement },
        };

        public static int RecupererIcone(int itemId, int modelIndex = 0)
        {
            var item = LifeManager.instance.item.GetItem(itemId);
            if (item == null || item.models == null || item.models.Count == 0)
            {
                return RecupererIcone(1112);
            }

            if (modelIndex < 0 || modelIndex >= item.models.Count)
            {
                modelIndex = 0;
            }

            int iconId = Array.IndexOf(LifeManager.instance.icons, item.models[modelIndex]?.icon);

            if ((iconId == default || iconId < 0) && Exceptions.ContainsKey(itemId))
            {
                iconId = Exceptions[itemId];
            }

            return iconId >= 0 ? iconId : RecupererIcone(1112);
        }
    }
}
