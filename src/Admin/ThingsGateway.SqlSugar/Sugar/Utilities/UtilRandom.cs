public static class UtilRandom
{
    private static readonly Random Random = new Random();

    /// <summary>
    /// 从权重字典中按权重随机选择一个 key（下标）
    /// </summary>
    /// <param name="pars">key = 下标，value = HitRate（权重）</param>
    /// <returns>命中的 key（下标）</returns>
    public static int GetRandomIndex(Dictionary<int, int> pars)
    {
        if (pars == null || pars.Count == 0)
            throw new ArgumentException("权重字典不能为空", nameof(pars));

        var list = pars.ToList(); // 避免多次 ToList
        int maxValue = 0;

        foreach (var item in list)
        {
            maxValue += item.Value;
        }

        if (maxValue == 0)
            throw new InvalidOperationException("所有权重为0");

        int num = Random.Next(1, maxValue); // 注意：左闭右开 [1, maxValue)

        int endValue = 0;

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            int beginValue = endValue;
            endValue += item.Value;

            if (num >= beginValue && num < endValue)
                return item.Key;
        }

        return list[0].Key; // 如果没有命中，返回第一个 key（下标）
    }
}
