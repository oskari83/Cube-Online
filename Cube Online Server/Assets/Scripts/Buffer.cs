public class Buffer{
    private bool[] inputs = new bool[5];
    private ushort tick = 0;
    private ushort batchTick = 1;
    private ushort nInputs;

    public Buffer(ushort amountOfInputs){
        nInputs = amountOfInputs;
    }

    public void incrementTick(){
        tick +=1;
    }

    public ushort getTick(){
        return tick;
    }
    
    public ushort getBatchTick(){
        return batchTick;
    }

    public void setInputs(bool[] inp){
        for(int i=0; i < 5; i++){
            inputs[i]=inp[i];
        }
    }

    public bool[] getInputs(){
        tick=0;
        incBatch();
        return inputs;
    }

    private void incBatch(){
        batchTick +=1;
    }
}