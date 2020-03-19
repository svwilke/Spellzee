public class SummonComponent : SpellComponent {

	protected PawnTemplate pawnTemplate;
	protected Attribute summonCount;
	protected Attribute summonLevel;
	protected Attribute baseHpMultiplier;
	protected bool isMinion;
	protected bool casterTeam;
	protected int maxSummonCount;

	public SummonComponent(PawnTemplate pawnTemplate, int baseCount, int baseLevel, int baseHpMulti, int maxSummon = 1, bool isMinion = true, bool casterTeam = true) : base() {
		this.pawnTemplate = pawnTemplate;
		summonCount = new Attribute().SetBaseValue(baseCount);
		summonLevel = new Attribute().SetBaseValue(baseLevel);
		baseHpMultiplier = new Attribute().SetBaseValue(baseHpMulti);
		maxSummonCount = maxSummon;
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
		for(int i = 0; i < count && IsCastable(spell, context); i++) {
			Pawn summon = CreateSummon(spell, context);
			context.GetBattle().CmdAddPawn(summon);
			IncreaseSummonCount(spell, summon);
		}
	}

	public Pawn CreateSummon(Spell spell, RollContext context) {
		Pawn.Team team = GetTeam(context);
		int level = (int)summonLevel.GetValue();
		int hpMulti = (int)baseHpMultiplier.GetValue();
		Pawn pawn = GetPawnTemplate().Create(hpMulti, level, team);
		pawn.SetOwner(context.GetCaster());
		pawn.OnDeath.AddListener(p => ReduceSummonCount(spell, p));
		if(IsMinion()) {
			pawn.SetMinion();
		}
		return pawn;
	}

	public void IncreaseSummonCount(Spell spell, Pawn pawn) {
		SpellData sd = pawn.GetOwner().GetSpellData(spell);
		int currentSummonCount = sd.GetInt(DataKey.SummonCount);
		sd.SetInt(DataKey.SummonCount, currentSummonCount + 1);
		pawn.GetOwner().Synchronize();
	}

	public void ReduceSummonCount(Spell spell, Pawn pawn) {
		SpellData sd = pawn.GetOwner().GetSpellData(spell);
		int currentSummonCount = sd.GetInt(DataKey.SummonCount);
		sd.SetInt(DataKey.SummonCount, currentSummonCount > 0 ? currentSummonCount - 1 : 0);
		pawn.GetOwner().Synchronize();
	}

	public Pawn.Team GetTeam(RollContext context) {
		Pawn.Team team = context.GetTeam();
		if(!casterTeam) {
			team = team == Pawn.Team.Friendly ? Pawn.Team.Hostile : Pawn.Team.Friendly;
		}
		return team;
	}

	public override bool IsCastable(Spell spell, RollContext context) {
		if(maxSummonCount >= 1 && GetSummonedCount(spell, context) >= maxSummonCount) {
			return false;
		}
		return context.GetBattle().GetSlotForTeam(GetTeam(context)) >= 0;
	}

	private int GetSummonedCount(Spell spell, RollContext context) {
		return context.GetCaster().GetSpellData(spell).GetInt(DataKey.SummonCount);
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		int count = (int)summonCount.GetValue();
		int level = (int)summonLevel.GetValue();
		string number = count == 0 ? "no" : count == 1 ? "a" : count.ToString();
		return "Summon " + number + " level " + (level + 1) + " " + pawnTemplate.GetName() + ".";
	}
}
