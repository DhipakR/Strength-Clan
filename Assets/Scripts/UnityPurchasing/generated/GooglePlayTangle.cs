// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("rgjWQh8nukd46hlYCtBhcVCe6oYbVA28/vyNe4aTw/r57aDw+0FPCJATHRIikBMYEJATExKN5MHyCMVmLY6tIrQijtb8j4Z1eklPor6nKgbifRPUgpFoyoVSYQwgX8YsrGnEkftob0q8e6oOBBrLJdN5wJAsgeE+XIqrX0iCBjDPUUxi28GLTDS/K4sUveVQNEfUphikhk2bTQxvalID0u4FKvd3ASMctGHqZzrrl0adtLHaklWnyKpkQQSdNukCo2+SWQvRPKsikBMwIh8UGziUWpTlHxMTExcSEellBq+sI2Eoxnsn0fLUcqvA/On1BBHfNShr233Gvtr4I4H5T8WHZEcmS77XxXvTktmK3oBKHmJhRZPJDrfEYkVVM6XgXRARExIT");
        private static int[] order = new int[] { 13,9,9,4,10,10,12,7,12,9,13,12,13,13,14 };
        private static int key = 18;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
