using System;

[Serializable]
public class WeaponInstance
{
    public WeaponProfile profile;
    public int enhanceLevel;
    public int level;
    public WeaponInstance(WeaponProfile profile)
    {
        this.profile = profile;
        this.enhanceLevel = 0;
    }
    public WeaponInstance(WeaponProfile profile, int enhanceLevel)
    {
        this.profile = profile;
        this.enhanceLevel = enhanceLevel;
    }
    public WeaponInstance(WeaponProfile profile, int level, int enhanceLevel)
    {
        this.profile = profile;
        this.level = level;
        this.enhanceLevel = enhanceLevel;
    }

}