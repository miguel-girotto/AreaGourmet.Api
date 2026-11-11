namespace AreaGourmet.Services;

public static class ProductImages
{
    public static readonly IReadOnlyDictionary<string, string> Opcoes = new Dictionary<string, string>
    {
        ["meal.jpg"] = "Arroz, feijão e farofa",
        ["pureDeBatata.jpg"] = "Pure de Batata caseiro",
        ["suco-uva.jpg"] = "suco de uva natural",
        ["OvoFrito.jpg"] = "Ovo",
        ["marmitaVegetariana.jpg"] = "Marmita Vegetarina fresca",
        ["strogonoffCarne.jpg"] = "Marmita Strogonoff de carne",
        ["strogonoffFrango.jpg"] = "Marmita Strogonoff de frango",
        ["marmitaExecutiva.jpg"] = "Marmita executiva",
        ["marmitaSaudavel.jpg"] = "Marmita Saudavel",
        ["marmitaFrangoGrelhado.jpg"] = "Marmita frango grelhado",
        ["suco-natural-de-laranja.jpg"] = "Suco de laranja teste",
        ["refriGuarana.jpg"] = "Refrigerante Guarana",
        ["farofaCrocrante.jpg"] = "Farofa Crocrante",
        ["marmitaPequena.jpg"] = "Marmita Pequena P",
        ["marmitaGrande.jpg"] = "Marmita Grande G",
        ["fitness.jpg"] = "Salada com frango",
        ["chicken.jpg"] = "Frango grelhado",
        ["beans.jpg"] = "Feijoada",
        ["beans - Copia.jpg"] = "Feijoada2",
        ["juice.jpg"] = "Suco de laranja",
        ["soda.jpg"] = "Guaraná",
        ["extra.jpg"] = "Farofa",
        ["stroganoff.jpg"] = "Strogonoff de carne",
        ["vegetarian.jpg"] = "Arroz com vegetais",
        ["grape.jpg"] = "Suco de uva",
        ["cola.jpg"] = "Refrigerante de cola",
        ["water.jpg"] = "Água mineral",
        ["puree.jpg"] = "Purê de batata",
        ["egg.jpg"] = "Ovo frito"
    };
    private static readonly IReadOnlyDictionary<string, string> Legacy = new Dictionary<string, string>
    {
        ["meal.svg"] = "meal.jpg",
        ["fitness.svg"] = "fitness.jpg",
        ["chicken.svg"] = "chicken.jpg",
        ["beans.svg"] = "beans.jpg",
        ["juice.svg"] = "juice.jpg",
        ["soda.svg"] = "soda.jpg",
        ["extra.svg"] = "extra.jpg"
    };
    public static string Normalizar(string? nome) => nome != null && Legacy.TryGetValue(nome, out var atual) ? atual : nome ?? "meal.jpg";
    public static bool Valida(string? nome) => nome != null && Opcoes.ContainsKey(Normalizar(nome));
    public static string Url(string? nome) => "/images/photos/" + (Valida(nome) ? Normalizar(nome) : "meal.jpg");
}
