using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using static AttributeModifier;

public class Pawn {

	public EventBus.EvtDamageHeal OnTakeDamage = new EventBus.EvtDamageHeal();
	public EventBus.EvtDamageHeal OnHeal = new EventBus.EvtDamageHeal();
	public EventBus.EvtPawnOnly OnDeath = new EventBus.EvtPawnOnly();
	public EventBus.EvtPawn OnSetupTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnBeginTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnEndTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnSpellsChange = new EventBus.EvtPawn();
	public EventBus.EvtPawnString OnItemEquipped = new EventBus.EvtPawnString();
	public EventBus.EvtPawnString OnItemUnequipped = new EventBus.EvtPawnString();
	public EventBus.EvtPawnString OnItemUsed = new EventBus.EvtPawnString();

	public EventBus.EvtSpellComponent OnSpellComponentCaster = new EventBus.EvtSpellComponent();
	public EventBus.EvtSpellComponent OnSpellComponentTarget = new EventBus.EvtSpellComponent();
	public EventBus.EvtSpellComponentList OnBuildSpellComponents = new EventBus.EvtSpellComponentList();

	public EventBus.EvtSpell OnBeforeSpellCast = new EventBus.EvtSpell();
	public EventBus.EvtSpell OnAfterSpellCast = new EventBus.EvtSpell();

	private int id;
	private string name;

	private string spriteName;

	public enum Team { Friendly, Hostile }
	public Team team { get; private set; }

	public int CurrentHp { get; private set; }
	public Attribute MaxHp = new Attribute();

	public int Level { get; private set; }
	public int CurrentXP { get; private set; }
	public int NextLevelXP { get { return progression.GetRequiredXP(Level + 1); } }
	public float XPProgress { get { return (CurrentXP - progression.GetRequiredXP(Level)) / (float)(progression.GetRequiredXP(Level + 1) - progression.GetRequiredXP(Level)); } }

	private int xpGain = 1;

	private PawnProgression progression;

	private List<string> knownSpells = new List<string>();
	private bool isDead = false;

	private Dictionary<string, int> ailments = new Dictionary<string, int>();
	private List<Status> statusList = new List<Status>();

	private HashSet<string> equipped = new HashSet<string>();

	private Dictionary<string, SpellData> spellData = new Dictionary<string, SpellData>();

	public Attribute HitChance = new Attribute().SetBaseValue(1);
	public Attribute SpellHealBonus = new Attribute();
	public Attribute SpellDamageBonus = new Attribute();

	public Attribute AilmentApplyBonus = new Attribute();
	public Attribute DamageReduction = new Attribute();

	public Attribute[] Affinities = new Attribute[Element.Count];
	public Attribute DieCount = new Attribute().SetBaseValue(5);
	public Attribute LockCount = new Attribute();
	public Attribute RollCount = new Attribute().SetBaseValue(3);
	public Attribute SpellSlotCount = new Attribute().SetBaseValue(4);
	public Attribute EndOfBattleRestoration = new Attribute(new AttributeModifier("Base Value", Operation.MultiplyTotal, 0.125));

	private AIModule ai;

	private bool isMinion = false;
	private Pawn owner = null;

	public Pawn(string name, int maxHp, Team team) {
		this.name = name;
		MaxHp.SetBaseValue(maxHp);
		CurrentHp = GetMaxHp();
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i] = new Attribute();
		}
		this.team = team;
	}

	public void SetProgression(PawnProgression progression) {
		this.progression = progression;
	}

	public PawnProgression GetProgression() {
		return progression;
	}

	public void AddXP(int xp) {
		CurrentXP += xp;
		if(progression != null && CurrentXP >= NextLevelXP) {
			LevelUp();
		}
	}

	public void SetXPGain(int xp) {
		xpGain = xp;
	}

	public int GetXPGain() {
		return xpGain;
	}

	private void LevelUp() {
		Level += 1;
	}

	public void SetLevel(int level) {
		Level = level;
	}

	public Pawn SetMinion() {
		isMinion = true;
		return this;
	}

	public bool IsMinion() {
		return isMinion;
	}

	public Pawn SetOwner(Pawn owner) {
		this.owner = owner;
		return this;
	}

	public Pawn GetOwner() {
		return owner;
	}

	public void SetSprite(string spriteName) {
		this.spriteName = spriteName;
	}

	public string GetSprite() {
		return spriteName;
	}

	public void SetId(int id) {
		this.id = id;
	}

	public int GetId() {
		return id;
	}

	public void Equip(Equipment eq) {
		Equip(eq.GetId());
	}

	public void Equip(string eqId) {
		equipped.Add(eqId);
		Equipments.Get(eqId).OnEquipped(this);
		OnItemEquipped.Invoke(null, this, eqId);
	}

	public void Unequip(Equipment eq) {
		Unequip(eq.GetId());
	}

	public bool HasEquipped(Equipment eq) {
		return HasEquipped(eq.GetId());
	}

	public bool HasEquipped(string eqId) {
		return equipped.Contains(eqId);
	}

	public void Unequip(string eqId) {
		equipped.Remove(eqId);
		Equipments.Get(eqId).OnUnequipped(this);
		OnItemUnequipped.Invoke(null, this, eqId);
	}

	public string[] GetEquipment() {
		return equipped.ToArray();
	}

	public void AddStatus(Status status) {
		foreach(Status s in statusList) {
			if(s.GetType() == status.GetType() && s.Merge(status)) {
				return;
			}
		}
		statusList.Add(status);
		status.OnAdded(this);
	}

	public void RemoveStatus(Status status) {
		statusList.Remove(status);
		status.OnRemoved(this);
	}

	public void RemoveAllStatuses() {
		List<Status> statuses = new List<Status>(statusList);
		foreach(Status s in statuses) {
			RemoveStatus(s);
		}
	}

	public void SetStatuses(List<Status> statusList) {
		this.statusList = statusList;
		// Maybe change this to remove all statuses
		// and then add these to achieve sync with server
	}

	public void CmdAddStatus(Status status) {
		AddStatus(status);
		CmdUpdateStatuses();
	}

	public void CmdRemoveStatus(Status status) {
		RemoveStatus(status);
		CmdUpdateStatuses();
	}

	public void CmdUpdateStatuses() {
		NetworkServer.SendToAll(GameMsg.UpdateAilment, new GameMsg.MsgStatusList() { pawnId = id, statuses = statusList });
	}

	public List<Status> GetStatuses() {
		return statusList;
	}

	public int GetMaxHp() {
		return (int)MaxHp.GetValue();
	}

	public void Damage(int dmg) {
		if(dmg < 0) {
			dmg = 0;
		}
		CurrentHp -= dmg;
		if(CurrentHp <= 0) {
			Die();
		}
	}

	public void Heal(int heal) {
		if(heal < 0) {
			heal = 0;
		}
		CurrentHp += heal;
		if(CurrentHp > GetMaxHp()) {
			CurrentHp = GetMaxHp();
		}
	}

	public void CmdDamage(EventBus.DamageHealEvent damage) {
		OnTakeDamage.Invoke(this, damage);
		int dmg = damage.amount;
		if(dmg < 0) {
			dmg = 0;
		}
		Damage(dmg);
		NetworkServer.SendToAll(GameMsg.TakeDamage, new GameMsg.MsgIntegerArray(id, dmg));
		//NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void CmdHeal(EventBus.DamageHealEvent heal) {
		OnHeal.Invoke(this, heal);
		int hl = heal.amount;
		if(hl < 0) {
			hl = 0;
		}
		Heal(hl);
		NetworkServer.SendToAll(GameMsg.Heal, new GameMsg.MsgIntegerArray(id, hl));
		//NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void Restore() {
		int restoration = (int)EndOfBattleRestoration.GetValue((int)MaxHp.GetValue());
		if(restoration <= 0) {
			restoration = 1;
		}
		if(isDead) {
			CurrentHp = restoration;
			isDead = false;
		} else {
			Heal(restoration);
		}
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void FullRestore() {
		CurrentHp = GetMaxHp();
		RemoveAllStatuses();
	}

	public void Die() {
		isDead = true;
		OnDeath.Invoke(this);
	}

	public void Revive() {
		if(isDead && CurrentHp > 0) {
			isDead = false;
		}
	}

	public void CmdRevive() {
		Revive();
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public bool IsAlive() {
		return !isDead;
	}

	public string GetName() {
		return name;
	}

	public void AddSpell(string spellId) {
		if(!knownSpells.Contains(spellId)) {
			knownSpells.Add(spellId);
		}
	}

	public void AddSpell(Spell spell) {
		AddSpell(spell.GetId());
	}

	public void RemoveSpell(string spellId) {
		if(knownSpells.Contains(spellId)) {
			knownSpells.Remove(spellId);
		}
	}

	public Spell[] GetSpells() {
		return knownSpells.Select((id) => Spells.Get(id)).ToArray();
	}

	public List<string> GetKnownSpellIds() {
		return new List<string>(knownSpells);
	}

	public List<string> GetKnownSpellIdsMutable() {
		return knownSpells;
	}

	public bool DoesKnowSpell(Spell spell) {
		return DoesKnowSpell(spell.GetId());
	}

	public bool DoesKnowSpell(string spellId) {
		return knownSpells.Contains(spellId);
	}

	public double GetAffinity(Element element) {
		return GetAffinity(element.GetId());
	}

	public double GetAffinity(int elemId) {
		return UnityEngine.Mathf.Max((int)Affinities[elemId].GetValue(Element.All[elemId].GetBaseAffinity()), 0);
	}

	public double GetAffinityTotal() {
		double total = 0;
		for(int i = 0; i < Affinities.Length; i++) {
			total += GetAffinity(i);
		}
		return total;
	}
	
	public Pawn SetAI(AIModule ai) {
		this.ai = ai;
		return this;
	}

	public bool HasAI() {
		return ai != null;
	}

	public AIModule GetAI() {
		return ai;
	}

	public void Synchronize() {
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public SpellData GetSpellData(Spell spell) {
		return GetSpellData(spell.GetId());
	}

	public SpellData GetSpellData(string spellId) {
		if(spellData.ContainsKey(spellId)) {
			return spellData[spellId];
		} else {
			return spellData[spellId] = new SpellData(); // possibly replace with Spell.CreateSpellData()
		}
	}

	public virtual void Serialize(NetworkWriter writer) {
		writer.Write(id);
		writer.Write(name);
		writer.Write(spriteName);
		writer.Write((byte)team);
		writer.Write(isMinion);
		writer.Write(CurrentHp);
		MaxHp.Serialize(writer);
		writer.Write(progression.GetId());
		writer.Write(CurrentXP);
		writer.Write(Level);
		writer.Write(xpGain);
		writer.Write(isDead);
		writer.Write(knownSpells.Count);
		for(int i = 0; i < knownSpells.Count; i++) {
			writer.Write(knownSpells[i]);
		}
		writer.Write(statusList.Count);
		foreach(Status status in statusList) {
			status.Serialize(writer);
		}
		writer.Write(equipped.Count);
		foreach(string eqId in equipped) {
			writer.Write(eqId);
		}
		writer.Write(spellData.Count);
		foreach(KeyValuePair<string, SpellData> kvp in spellData) {
			writer.Write(kvp.Key);
			kvp.Value.Serialize(writer);
		}
		HitChance.Serialize(writer);
		SpellHealBonus.Serialize(writer);
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i].Serialize(writer);
		}
		DieCount.Serialize(writer);
		RollCount.Serialize(writer);
		SpellSlotCount.Serialize(writer);
		EndOfBattleRestoration.Serialize(writer);
	}

	public virtual void Deserialize(NetworkReader reader) {
		id = reader.ReadInt32();
		name = reader.ReadString();
		spriteName = reader.ReadString();
		team = (Team)reader.ReadByte();
		isMinion = reader.ReadBoolean();
		CurrentHp = reader.ReadInt32();
		MaxHp.Deserialize(reader);
		progression = PawnProgressions.Get(reader.ReadString());
		CurrentXP = reader.ReadInt32();
		Level = reader.ReadInt32();
		xpGain = reader.ReadInt32();
		isDead = reader.ReadBoolean();
		int knownSpellCount = reader.ReadInt32();
		knownSpells = new List<string>(knownSpellCount);
		for(int i = 0; i < knownSpellCount; i++) {
			AddSpell(reader.ReadString());
		}
		int statusCount = reader.ReadInt32();
		statusList = new List<Status>(statusCount);
		for(int i = 0; i < statusCount; i++) {
			statusList.Add(Status.DeserializeNew(reader));
		}
		equipped.Clear();
		int eqCount = reader.ReadInt32();
		for(int i = 0; i < eqCount; i++) {
			Equip(reader.ReadString());
		}
		spellData.Clear();
		int spellDataCount = reader.ReadInt32();
		for(int i = 0; i < spellDataCount; i++) {
			spellData[reader.ReadString()] = SpellData.DeserializeNew(reader);
		}
		HitChance.Deserialize(reader);
		SpellHealBonus.Deserialize(reader);
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i].Deserialize(reader);
		}
		DieCount.Deserialize(reader);
		RollCount.Deserialize(reader);
		SpellSlotCount.Deserialize(reader);
		EndOfBattleRestoration.Deserialize(reader);
	}

	public static Pawn DeserializeNew(NetworkReader reader) {
		Pawn p = new Pawn("", 1, Team.Friendly);
		p.Deserialize(reader);
		return p;
	}

	public static Pawn CreatePlayer(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		return PawnTemplates.GetPlayableClasses()[lobbyPlayer.charClass].Create(lobbyPlayer);
	}
	/*
	public static Pawn Clone(Pawn other) {
		Pawn clone = new Pawn(other.name, other.MaxHp, other.team);
		clone.CurrentHp = other.CurrentHp;
		clone.id = other.id;
		clone.isDead = other.isDead;
		clone.isMinion = other.isMinion;
		clone.knownSpells = new List<string>(other.knownSpells.Count);
		clone.CurrentXP = other.CurrentXP;
		clone.Level = other.Level;
		foreach(string spellId in other.knownSpells) {
			clone.knownSpells.Add(spellId);
		}
		clone.ailments = new Dictionary<string, int>(other.ailments);
		clone.equipped = new HashSet<string>(other.equipped);
		clone.SpellHealBonus = Attribute.Clone(other.SpellHealBonus);
		return clone;
	}
	*/
}
