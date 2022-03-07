using System.Linq;

public static class Tools{

    static bool[] boolC1 = new bool[] {false,false,false,false};
    static bool[] boolC2 = new bool[] {true,false,false,false};
    static bool[] boolC3 = new bool[] {false,true,false,false};
    static bool[] boolC4 = new bool[] {false,false,true,false};
    static bool[] boolC5 = new bool[] {false,false,false,true};
    static bool[] boolC6 = new bool[] {true,true,false,false};
    static bool[] boolC7 = new bool[] {false,true,true,false};
    static bool[] boolC8 = new bool[] {false,false,true,true};
    static bool[] boolC9 = new bool[] {true,false,true,false};
    static bool[] boolC10 = new bool[] {true,false,false,true};
    static bool[] boolC11 = new bool[] {false,true,false,true};
    static bool[] boolC12 = new bool[] {true,true,true,false};
    static bool[] boolC13 = new bool[] {false,true,true,true};
    static bool[] boolC14 = new bool[] {true,false,true,true};
    static bool[] boolC15 = new bool[] {true,true,false,true};
    static bool[] boolC16 = new bool[] {true,true,true,true};


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

    public static byte BoolsToByte(bool[] bools){
        if(bools.SequenceEqual(boolC16)){
            return 16;
        }else if(bools.SequenceEqual(boolC2)){
            return 2;
        }else if(bools.SequenceEqual(boolC3)){
            return 3;
        }else if(bools.SequenceEqual(boolC4)){
            return 4;
        }else if(bools.SequenceEqual(boolC5)){
            return 5;
        }else if(bools.SequenceEqual(boolC6)){
            return 6;
        }else if(bools.SequenceEqual(boolC7)){
            return 7;
        }else if(bools.SequenceEqual(boolC8)){
            return 8;
        }else if(bools.SequenceEqual(boolC9)){
            return 9;
        }else if(bools.SequenceEqual(boolC10)){
            return 10;
        }else if(bools.SequenceEqual(boolC11)){
            return 11;
        }else if(bools.SequenceEqual(boolC12)){
            return 12;
        }else if(bools.SequenceEqual(boolC13)){
            return 13;
        }else if(bools.SequenceEqual(boolC14)){
            return 14;
        }else if(bools.SequenceEqual(boolC15)){
            return 15;
        }else {
            return 1;
        }
    }

    public static bool[] ByteToBools(byte bt){
        switch(bt){
            case 1:
                return boolC1;
            case 2:
                return boolC2;
            case 3:
                return boolC3;
            case 4:
                return boolC4;
            case 5:
                return boolC5;
            case 6:
                return boolC6;
            case 7:
                return boolC7;
            case 8:
                return boolC8;
            case 9:
                return boolC9;
            case 10:
                return boolC10;
            case 11:
                return boolC11;
            case 12:
                return boolC12;
            case 13:
                return boolC13;
            case 14:
                return boolC14;
            case 15:
                return boolC15;
            case 16:
                return boolC16;
            default:
                return boolC1;
        }
    }
}