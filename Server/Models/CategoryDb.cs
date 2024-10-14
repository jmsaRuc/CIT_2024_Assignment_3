using System;

namespace Server.Models;

public class CategoryDb
{
    public Category[] categories;

    public CategoryDb()
    {   
        categories = GetCategories();
    }

    public Category? GetCategoryCid(int cid)
    {
        foreach (var category in categories)
        {
            if (category.cid == cid)
            {
                return category;
            }
        }
        return null;
    }

    public Category? GetCategoryName(string name)
    {
        foreach (var category in categories)
        {
            if (category.name == name)
            {
                return category;
            }
        }
        return null;
    }

    public Category[] GetCategories()
    {
        return categories;
    }

    public void AddCategory(Category category)
    {
        Array.Resize(ref categories, categories.Length + 1);
        categories[^1] = category;

    }

    public void UpdateCategory(Category category)
    {
        for (int i = 0; i < categories.Length; i++)
        {
            if (categories[i].cid == category.cid)
            {
                categories[i] = category;
                return;
            }
        }
    }

    public void DeleteCategoryCid(int cid)
    {
        for (int i = 0; i < categories.Length; i++)
        {
            if (categories[i].cid == cid)
            {
                for (int j = i; j < categories.Length - 1; j++)
                {
                    categories[j] = categories[j + 1];
                }
                Array.Resize(ref categories, categories.Length - 1);
                return;
            }
        }
    }

    public void DeleteCategoryNameCid(string name)
    {
        for (int i = 0; i < categories.Length; i++)
        {
            if (categories[i].name == name)
            {
                for (int j = i; j < categories.Length - 1; j++)
                {
                    categories[j] = categories[j + 1];
                }
                Array.Resize(ref categories, categories.Length - 1);
                return;
            }
        }
    }

    public void saveCategories(){
        string json = System.Text.Json.JsonSerializer.Serialize(categories);
        System.IO.File.WriteAllText("categories.json", json);
    }

    public void loadCategories(){
        string json = System.IO.File.ReadAllText("categories.json");
        categories = System.Text.Json.JsonSerializer.Deserialize<Category[]>(json) ?? Array.Empty<Category>();
    }

}
