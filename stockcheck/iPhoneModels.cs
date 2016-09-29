using System.Collections.Generic;
using System.Text;

namespace stockcheck {
    public class IphoneModel {

        public IphoneModel(IphoneSize iPhoneSize, StorageSize size, Colour colour) {
            IphoneSize = iPhoneSize;
            StorageSize = size;
            Colour = colour;
        }

        public IphoneSize IphoneSize { get; }

        public StorageSize StorageSize { get; }

        public Colour Colour { get; }

        public static string[] modelIdentifiers = new[] {
            "?", "9C", "96", "8X", "92", "97", "8Y", "93", "98", "90", "94", "99", "9A", "91", "95",
            "?", "4V", "51", "4M", "4W", "QM", "4P", "4X", "QN", "4Q", "4Y", "QP", "4U", "50", "QQ"
        };

        public string ToIdentifier() {
            StringBuilder builder = new StringBuilder("MN");
            int modelIndex = 15 * (int)IphoneSize + 3 * (int)Colour + (int)StorageSize;
            return $"MN{modelIdentifiers[modelIndex]}2B/A";
        }

        public string ToDisplayName() {
            StringBuilder displayName = new StringBuilder();
            switch(IphoneSize) {
                case IphoneSize.iphone7:
                    displayName.Append("iPhone 7 ");
                    break;
                case IphoneSize.iphone7Plus:
                    displayName.Append("iPhone 7 Plus ");
                    break;
            }
            switch(Colour) {
                case Colour.JetBlack:
                    displayName.Append("Jet Black, ");
                    break;
                case Colour.Black:
                    displayName.Append("Black, ");
                    break;
                case Colour.Silver:
                    displayName.Append("Silver, ");
                    break;
                case Colour.Gold:
                    displayName.Append("Gold, ");
                    break;
                case Colour.RoseGold:
                    displayName.Append("Rose Gold, ");
                    break;
            }
            switch(StorageSize) {
                case StorageSize.GB32:
                    displayName.Append("32GB");
                    break;
                case StorageSize.GB128:
                    displayName.Append("128GB");
                    break;
                case StorageSize.GB256:
                    displayName.Append("256GB");
                    break;
            }
            return displayName.ToString();
        }

        public static IEnumerable<IphoneModel> GetModels(IEnumerable<IphoneSize> iphoneSizes, IEnumerable<StorageSize> sizes, IEnumerable<Colour> colours) {
            foreach(var iphoneSize in iphoneSizes) {
                foreach(var size in sizes) {
                    foreach(var colour in colours) {
                        if(colour != Colour.JetBlack || size != StorageSize.GB32) {
                            yield return new IphoneModel(iphoneSize, size, colour);
                        }
                    }
                }
            }
        }
    }

    public enum IphoneSize {
        iphone7 = 0,
        iphone7Plus = 1
    }

    public enum StorageSize {
        GB32 = 0,
        GB128 = 1,
        GB256 = 2
    }

    public enum Colour {
        JetBlack = 0,
        Black = 1,
        Silver = 2,
        Gold = 3,
        RoseGold = 4
    }
}
