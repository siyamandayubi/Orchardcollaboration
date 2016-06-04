using Orchard.Data.Migration;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.CRM.ProjectSetup
{
    public class Migrations : DataMigrationImpl
    {
        private readonly Lazy<IRecipeManager> recipeManager;
        private readonly Lazy<IRecipeHarvester> recipeHarvester;

        public Migrations(Lazy<IRecipeManager> recipeManager, Lazy<IRecipeHarvester> recipeHarvester)
        {
            this.recipeHarvester = recipeHarvester;
            this.recipeManager = recipeManager;
        }

        public int Create()
        {
            Recipe recipe = this.recipeHarvester.Value.HarvestRecipes("Orchard.CRM.ProjectSetup").FirstOrDefault(x => x.Name == "orchardcollaboration");

            if (recipe != null)
            {
                this.recipeManager.Value.Execute(recipe);
            }
            
            return 1;
        }
    }
}