public static class TesseractLanguage
{
    public enum Languages
    {
        English,
        German,
        French,
        Spanish,
        Italian,
        Dutch,
        Portuguese,
        Russian,
        Japanese,
        ChineseSimplified,
        ChineseTraditional,
        Korean,
        Arabic,
        Turkish,
        Swedish,
        Polish,
        Greek,
        Hungarian,
        Finnish,
        Danish,
        Norwegian
    }

    public static string GetLanguageCode(Languages language)
    {
        switch (language)
        {
            case Languages.English:
                return "eng";
            case Languages.German:
                return "deu";
            case Languages.French:
                return "fra";
            case Languages.Spanish:
                return "spa";
            case Languages.Italian:
                return "ita";
            case Languages.Dutch:
                return "nld";
            case Languages.Portuguese:
                return "por";
            case Languages.Russian:
                return "rus";
            case Languages.Japanese:
                return "jpn";
            case Languages.ChineseSimplified:
                return "chi_sim";
            case Languages.ChineseTraditional:
                return "chi_tra";
            case Languages.Korean:
                return "kor";
            case Languages.Arabic:
                return "ara";
            case Languages.Turkish:
                return "tur";
            case Languages.Swedish:
                return "swe";
            case Languages.Polish:
                return "pol";
            case Languages.Greek:
                return "ell";
            case Languages.Hungarian:
                return "hun";
            case Languages.Finnish:
                return "fin";
            case Languages.Danish:
                return "dan";
            case Languages.Norwegian:
                return "nor";
            default:
                throw new ArgumentException($"Unsupported language: {language}");
        }
    }
}