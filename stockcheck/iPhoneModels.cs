using System;
using System.Collections.Generic;
using System.Text;

namespace stockcheck {
    public class IphoneModel {

        public IphoneModel(PhoneSize iPhoneSize, StorageSize size, Colour colour) {
            PhoneSize = iPhoneSize;
            StorageSize = size;
            Colour = colour;
        }

        public PhoneSize PhoneSize { get; }

        public StorageSize StorageSize { get; }

        public Colour Colour { get; }

	    public static string[] ModelIdentifiers = {

		    //32  128  256

		    // == iPhone 7 ==
		    null, "96", "9C", // jet black
		    "8X", "92", "97", // black
		    "8Y", "93", "98", // silver
		    "90", "94", "99", // gold
		    "91", "95", "9A", // rose gold

		    // == iPhone 7 Plus ==
		    null, "4V", "51", // jet black
		    "QM", "4M", "4W", // black
		    "QN", "4P", "4X", // silver
		    "QP", "4Q", "4Y", // gold
		    "QQ", "4U", "50" // rose gold
	    };

	    public string ToIdentifier() => $"MN{ModelIdentifiers[(int)PhoneSize + (int)Colour + (int)StorageSize]}2B/A";

        public string ToDisplayName() {
            StringBuilder displayName = new StringBuilder();
            switch(PhoneSize) {
                case PhoneSize.iPhone7:
                    displayName.Append("iPhone 7 ");
                    break;
                case PhoneSize.iPhone7Plus:
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
                case StorageSize.Small:
                    displayName.Append("32GB");
                    break;
                case StorageSize.Medium:
                    displayName.Append("128GB");
                    break;
                case StorageSize.Large:
                    displayName.Append("256GB");
                    break;
            }
            return displayName.ToString();
        }

        public static IEnumerable<IphoneModel> GetModels(IEnumerable<PhoneSize> iphoneSizes, IEnumerable<StorageSize> sizes, IEnumerable<Colour> colours) {
            foreach(var iphoneSize in iphoneSizes) {
                foreach(var size in sizes) {
                    foreach(var colour in colours) {
                        if(colour != Colour.JetBlack || size != StorageSize.Small) {
                            yield return new IphoneModel(iphoneSize, size, colour);
                        }
                    }
                }
            }
        }

	    public static StorageSize StorageSizeFromInt(int size) {
		    if(size == 32) {
			    return StorageSize.Small;
		    }
		    if(size == 128) {
			    return StorageSize.Medium;
		    }
		    if(size == 256) {
			    return StorageSize.Large;
		    }
			throw new InvalidOperationException($"Size {size} is not a valid storage size.");
	    }
    }

    public enum PhoneSize {
	    // ReSharper disable InconsistentNaming
        iPhone7 = 0,
		iPhone7Plus = 15
		// ReSharper restore InconsistentNaming
	}

    public enum Colour {
        JetBlack = 0,
        Black = 3,
        Silver = 6,
        Gold = 9,
        RoseGold = 12
    }

    public enum StorageSize {
        Small = 0,
        Medium = 1,
        Large = 2
    }
}
