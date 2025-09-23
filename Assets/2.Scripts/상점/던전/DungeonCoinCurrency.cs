using UnityEngine;

public class DungeonCoinCurrency : MonoBehaviour
{
    public int currentDungeonCoins { get; private set; } = 100;

    /// <summary>
    /// 던전 코인을 추가하는 메서드. (예: 몬스터 처치 시 호출)
    /// </summary>
    /// <param name="amount">추가할 코인 양</param>
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            currentDungeonCoins += amount;
        }
    }

    /// <summary>
    /// 던전 코인을 사용하는 메서드.
    /// </summary>
    /// <param name="amount">사용할 코인 양</param>
    /// <returns>코인 사용 성공 여부</returns>
    public bool SubtractCoins(int amount)
    {
        if (amount > 0 && currentDungeonCoins >= amount)
        {
            currentDungeonCoins -= amount;
            return true;
        }
        Debug.LogWarning("던전 코인이 부족합니다.");
        return false;
    }
}