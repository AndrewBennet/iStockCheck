using System;
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

	    public static string[] ModelIdentifiers = {

		    //32  128  256

		    // == iPhone 7 ==
		    "?", "96", "9C", // jet black
		    "8X", "92", "97", // black
		    "8Y", "93", "98", // silver
		    "90", "94", "99", // gold
		    "91", "95", "9A", // rose gold

		    // == iPhone 7 Plus ==
		    "?", "4V", "51", // jet black
		    "QM", "4M", "4W", // black
		    "QN", "4P", "4X", // silver
		    "QP", "4Q", "4Y", // gold
		    "QQ", "4U", "50" // rose gold
	    };

        public string ToIdentifier() {
	        int modelIndex = (int)IphoneSize + (int)Colour + (int)StorageSize;
            return $"MN{ModelIdentifiers[modelIndex]}2B/A";
        }

        public string ToDisplayName() {
            StringBuilder displayName = new StringBuilder();
            switch(IphoneSize) {
                case IphoneSize.iPhone7:
                    displayName.Append("iPhone 7 ");
                    break;
                case IphoneSize.iPhone7Plus:
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
                case StorageSize.Gb32:
                    displayName.Append("32GB");
                    break;
                case StorageSize.Gb128:
                    displayName.Append("128GB");
                    break;
                case StorageSize.Gb256:
                    displayName.Append("256GB");
                    break;
            }
            return displayName.ToString();
        }

        public static IEnumerable<IphoneModel> GetModels(IEnumerable<IphoneSize> iphoneSizes, IEnumerable<StorageSize> sizes, IEnumerable<Colour> colours) {
            foreach(var iphoneSize in iphoneSizes) {
                foreach(var size in sizes) {
                    foreach(var colour in colours) {
                        if(colour != Colour.JetBlack || size != StorageSize.Gb32) {
                            yield return new IphoneModel(iphoneSize, size, colour);
                        }
                    }
                }
            }
        }

	    public static StorageSize StorageSizeFromInt(int size) {
		    if(size == 32) {
			    return StorageSize.Gb32;
		    }
		    if(size == 128) {
			    return StorageSize.Gb128;
		    }
		    if(size == 256) {
			    return StorageSize.Gb256;
		    }
			throw new InvalidOperationException($"Size {size} is not a valid storage size.");
	    }
    }

    public enum IphoneSize {
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
        Gb32 = 0,
        Gb128 = 1,
        Gb256 = 2
    }
}
