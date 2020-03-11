public class SummonComponent : SpellComponent {

	protected PawnTemplate pawnTemplate;
	protected Attribute summonCount;
	protected Attribute summonLevel;
	protected Attribute baseHpMultiplier;
	protected bool isMinion;
	protected bool casterTeam;

	public SummonComponent(PawnTemplate pawnTemplate, int baseCount, int baseLevel, int baseHpMulti, bool isMinion = true, bool casterTeam = true) : base(TargetType.None) {
		this.pawnTemplate = pawnTemplate;
		summonCount = new Attribute().SetBaseValue(baseCount);
		summonLevel = new Attribute().SetBaseValue(baseLevel);
		baseHpMultiplier = new Attribute().SetBaseValue(baseHpMulti);
		this.isMinion = isMinion;
		this.casterTeam = casterTeam;
	}

	public PawnTemplate GetPawnTemplate() {
		return pawnTemplate;
	}

	public void SetPawnTemplate(PawnTemplate pawnTemplate) {
		this.pawnTemplate = pawnTemplate;
	}

	public bool IsMinion() {
		return isMinion;
	}

	public bool SummonForCaster() {
		return casterTeam;
	}

	public bool SummonForOpponent() {
		return !casterTeam;
	}

	public void SetSummonForCaster(bool summonForCaster) {
		casterTeam = summonForCaster;
	}

	public void SetMinion(bool isMinion) {
		this.isMinion = isMinion;
	}

	public virtual void ModifySummonCount(AttributeModifier.Operation operation, double modifier) {
		summonCount.AddModifier(new AttributeModifier(summonCount.Size.ToString(), operation, modifier));
	}

	public virtual void ModifySummonLevel(AttributeModifier.Operation operation, double modifier) {
		summonLevel.AddModifier(new AttributeModifier(summonLevel.Size.ToString(), operation, modifier));
	}

	public virtual void ModifyBaseHpMultiplier(AttributeModifier.Operation operation, double modifier) {
		baseHpMultiplier.AddModifier(new AttributeModifier(baseHpMultiplier.Size.ToString(), operation, modifier));
	}

	public override void Execute(Spell spell, RollContext context) {
		int count = (int)summonCount.GetValue();
		for(int i = 0; i < count; i++) {
			Pawn summon = CreateSummon(GetTeam(context));
			context.GetBattle().CmdAddPawn(summon);
		}
	}

	public Pawn CreateSummon(Pawn.Team team) {
		int level = (int)summonLevel.GetValue();
		int hpMulti = (int)baseHpMultiplier.GetValue();
		Pawn pawn = GetPawnTemplate().Create(hpMulti, level, team);
		if(IsMinion()) {
			pawn.SetMinion();
		}
		return pawn;
	}

	public Pawn.Team GetTeam(RollContext context) {
		Pawn.Team team = context.GetTeam();
		if(!casterTeam) {
			team = team == Pawn.Team.Friendly ? Pawn.Team.Hostile : Pawn.Team.Friendly;
		}
		return team;
	}

	public override bool IsCastable(Spell spell, RollContext context) {
		return context.GetBattle().GetSlotForTeam(GetTeam(context)) >= 0;
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		int count = (int)summonCount.GetValue();
		int level = (int)summonLevel.GetValue();
		string number = count == 0 ? "no" : count == 1 ? "a" : count.ToString();
		return "Summon " + number + " Level " + level + " " + pawnTemplate.GetName() + ".";
	}
}
