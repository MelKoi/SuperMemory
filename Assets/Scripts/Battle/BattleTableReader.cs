using System;
using CardTables;
using UnityEngine;

public static class BattleTableReader
{
    public static bool TryReadCharacter(int characterId, out string characterName, out int health, out int toughness)
    {
        characterName = null;
        health = BattleVitals.OpeningHealth;
        toughness = BattleVitals.OpeningToughness;
        if (characterId < 0)
        {
            return false;
        }

        try
        {
            if (!TableDatabase.TryGet("Charactor", out var table) || !table.Contains(characterId))
            {
                return false;
            }

            characterName = table.GetString(characterId, "name");
            health = table.GetInt(characterId, "health");
            toughness = table.GetInt(characterId, "body");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("角色数据读取失败：" + exception.Message);
            characterName = null;
            health = BattleVitals.OpeningHealth;
            toughness = BattleVitals.OpeningToughness;
            return false;
        }
    }

    public static bool TryReadWeapon(int weaponId, out string weaponName)
    {
        weaponName = null;
        if (weaponId < 0)
        {
            return false;
        }

        try
        {
            if (!TableDatabase.TryGet("Weapon", out var table) || !table.Contains(weaponId))
            {
                return false;
            }

            weaponName = table.GetString(weaponId, "name");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("武器数据读取失败：" + exception.Message);
            weaponName = null;
            return false;
        }
    }
}
