public interface IChargeSkill
{
    void ChargeBegin(SkillContext ctx);
    void ChargeTick(SkillContext ctx, float dt);
    void ChargeEnd(SkillContext ctx, bool released);
}
