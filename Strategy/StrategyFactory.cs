using System.IO;
using System.Reflection;

namespace PlantSim.Strategy;

/// <summary>
/// JSONファイル内の文字列キーに基づいて、対応する戦略クラスのインスタンスを返します。
/// 内蔵の基本戦略に加え、Pluginsフォルダから外部DLLを読み込み、動的に戦略を追加します。
/// </summary>
public class StrategyFactory
{
    private readonly Dictionary<string, IStrategy> _strategies;

    public StrategyFactory()
    {
        _strategies = new Dictionary<string, IStrategy>(StringComparer.OrdinalIgnoreCase);

        // 内蔵の基本戦略を登録するメソッドを呼び出し
        RegisterBuiltInStrategies();

        // Pluginsフォルダから外部の戦略プラグインを読み込む
        LoadPluginStrategies();
    }

    /// <summary>
    /// アプリケーションに標準で組み込まれている基本的な戦略を登録します
    /// </summary>
    private void RegisterBuiltInStrategies()
    {
        _strategies["Standard"] = new LifecycleStrategy();
        _strategies["Photosynthesis"] = new EnergyStrategy();
        _strategies["WindDispersal"] = new ReproductionStrategy();

        // Growth Strategies
        _strategies["Tree"] = new GrowthStrategy(GrowthType.Apical, apicalDominanceFactor: 0.7, branchingAngle: 45);
        _strategies["Conifer"] = new GrowthStrategy(GrowthType.Apical, apicalDominanceFactor: 0.9, branchingAngle: 70);
        _strategies["Shrub"] = new GrowthStrategy(GrowthType.Apical, apicalDominanceFactor: 0.4, branchingAngle: 50);
        _strategies["Herbaceous"] = new GrowthStrategy(GrowthType.Basal);
        _strategies["Vine"] = new GrowthStrategy(GrowthType.Vine);
        _strategies["Rosette"] = new GrowthStrategy(GrowthType.Rosette);
    }

    /// <summary>
    /// 'Plugins' フォルダをスキャンし、見つかったDLLからIStrategyを実装したクラスを動的に読み込みます
    /// </summary>
    private void LoadPluginStrategies()
    {
        try
        {
            string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (!Directory.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
                return; // プラグインフォルダがなければ作成して終了
            }

            foreach (var dllFile in Directory.GetFiles(pluginPath, "*.dll"))
            {
                var assembly = Assembly.LoadFrom(dllFile);
                var strategyTypes = assembly.GetTypes().Where(t =>
                    typeof(IStrategy).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in strategyTypes)
                {
                    // パラメータなしのコンストラクタを持つ戦略のみを対象とする
                    if (Activator.CreateInstance(type) is IStrategy strategyInstance)
                    {
                        // クラス名をキーとして登録 (例: "MyCustomGrowthStrategy")
                        _strategies[type.Name] = strategyInstance;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // エラーハンドリング (実際にはログに出力するなど)
            Console.WriteLine($"Error loading plugins: {ex.Message}");
        }
    }

    public IStrategy GetStrategy(string key)
    {
        if (string.IsNullOrEmpty(key) || !_strategies.TryGetValue(key, out var strategy))
        {
            // エラーメッセージをより分かりやすく
            throw new KeyNotFoundException($"Strategy with key '{key}' not found. Make sure the key in the JSON file matches a built-in strategy or a class name from a plugin DLL.");
        }
        return strategy;
    }
}
