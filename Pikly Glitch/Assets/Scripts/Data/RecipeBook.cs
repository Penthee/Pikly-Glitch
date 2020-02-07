using Pikl;
using Pikl.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Recipe Book", menuName = "Data/New Recipe Book")]
    public class RecipeBook : ScriptableObject {

        public List<Recipe> recipes = new List<Recipe>();

        public static RecipeBook CreateInstance(RecipeBook item) {
            var data = CreateInstance<RecipeBook>();
            return data;
        }
    }

    [System.Serializable]
    public class Recipe {
        public Item item;
        public List<Ingredient> ingredients = new List<Ingredient>();

        public bool Craftable() {
            foreach(Ingredient ingredient in ingredients) {
                if (Player.Player.I.inventory.ItemCount(ingredient.item.name) < ingredient.quantity)
                    return false;
            }

            return true;
        }

        public void Craft() {
            if (Craftable()) {
                foreach(Ingredient ingredient in ingredients) {
                    Player.Player.I.inventory.DeleteForCraft(ingredient.item, ingredient.quantity);
                }

                Player.Player.I.inventory.Add(item);
            } 
        }
    }

    [System.Serializable]
    public class Ingredient {
        public Item item;
        [Range(1, 5)]
        public int quantity;
    }
}