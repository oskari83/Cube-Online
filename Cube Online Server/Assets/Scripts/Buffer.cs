public class Buffer{
    private byte[] buffer = new byte[] {1,1,1,1,1};

    public void addInput(byte bit){
        byte[] temp = buffer;
        buffer[0] = bit;
        for(int i=0; i<4; i++){
            buffer[i+1]=temp[i];
        }
    }

    public byte[] getBuffer(){
        return buffer;
    }
}