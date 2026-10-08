// Copyright (c) HThinh.yy. Original game code; third-party assets retain their own licenses.
using System;
using UnityEngine;
namespace NongTrai
{
    public static class FarmSpecialCrops
    {
        public static void Install(FieldManager field)
        {
            if(field.crops==null||field.crops.Length<6||field.crops.Length>=8)return;
            var crops=new CropDefinition[8];Array.Copy(field.crops,crops,6);
            for(int i=0;i<2;i++)
            {
                var crop=UnityEngine.Object.Instantiate(crops[i==0?3:4]);
                crop.name=i==0?"CrystalPumpkin":"GoldenBerry";crop.displayName=i==0?"Bí pha lê":"Dâu hoàng kim";
                crop.specialProduct=76+i;crop.growthSeconds=FarmCropBalance.ForField(6+i).seconds;crop.yield=3;
                crop.fruitColor=i==0?new Color(.2f,.85f,1):new Color(1,.72f,.12f);crops[6+i]=crop;
            }
            field.crops=crops;
        }
        public static void BossLoot(int[] items){items[74]+=2;items[75]+=1;}
    }
}
