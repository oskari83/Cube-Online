public class LagSimulator{
    private float minLat;
    private float maxLat;
    private float packetLossChance;

    public LagSimulator(float latencyMin, float latencyMax, float percentagePacketLoss){
        minLat = latencyMin;
        maxLat = latencyMax;
        packetLossChance = percentagePacketLoss;
    }

    public float AddRandomLatency() { 
        return UnityEngine.Random.Range(minLat, maxLat);
    }

    public bool CheckPacketLoss(){
        if(UnityEngine.Random.value > (1-packetLossChance)){
            return true;
        }else{
            return false;
        }
    }
}