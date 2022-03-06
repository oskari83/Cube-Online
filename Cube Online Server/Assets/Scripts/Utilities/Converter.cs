public static class Converter{
    public static string BoolsToString(bool[] bools){
        string s = "";
        for(int i = 0; i < bools.Length; i++){
            if (bools[i]==true){
                s+="1";
            }else{
                s+="0";
            }
        }
        return s;
    } 
}