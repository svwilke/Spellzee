using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using static AttributeModifier;

public class Pawn {

	public EventBus.EvtDamageHeal OnTakeDamage = new EventBus.EvtDamageHeal();
	public EventBus.EvtDamageHeal OnHeal = new EventBus.EvtDamageHeal();
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

	private int id;
	private string name;

	private string spriteName;

	public int CurrentHp { get; private set; }
	public int MaxHp { get; private set; }

	private List<string> knownSpells = new List<string>();
	private bool isDead = false;

	private Dictionary<string, int> ailments = new Dictionary<string, int>();
	private List<Status> statusList = new List<Status>();

	private HashSet<string> equipped = new HashSet<string>();

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

	public Pawn(string name, int maxHp) {
		this.name = name;
		this.MaxHp = CurrentHp = maxHp;
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i] = new Attribute();
		}
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
		if(CurrentHp > MaxHp) {
			CurrentHp = MaxHp;
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
		int restoration = MaxHp / 8;
		if(this is PlayerPawn) {
			restoration = (int)(this as PlayerPawn).EndOfBattleRestoration.GetValue(MaxHp);
		}
		if(isDead) {
			CurrentHp = restoration;
			isDead = false;
		} else {
			Heal(restoration);
		}
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void Die() {
		isDead = true;
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
		return Affinities[elemId].GetValue(Element.All[elemId].GetBaseAffinity());
	}

	public double GetAffinityTotal() {
		double total = 0;
		for(int i = 0; i < Affinities.Length; i++) {
			total += GetAffinity(i);
		}
		return total;
	}

	public void Synchronize() {
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public virtual void Serialize(NetworkWriter writer) {
		if(this is PlayerPawn) {
			writer.Write((byte)1);
		} else
		if(this is EnemyPawn) {
			writer.Write((byte)2);
		} else {
			writer.Write((byte)0);
		}
		writer.Write(id);
		writer.Write(name);
		writer.Write(spriteName);
		writer.Write(CurrentHp);
		writer.Write(MaxHp);
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
		CurrentHp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
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
		Pawn p;
		int cls = reader.ReadByte();
		switch(cls) {
			default:
			case 0:
				p = new Pawn("", 1);
				break;
			case 1:
				p = new PlayerPawn("", 1);
				break;
			case 2:
				p = new EnemyPawn("", 1);
				break;
		}
		p.Deserialize(reader);
		return p;
	}

	public static Pawn CreatePlayer(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		return PlayerTemplates.GetPlayableClasses()[lobbyPlayer.charClass].Create(lobbyPlayer);
	}

	public static Pawn Clone(Pawn other) {
		Pawn clone = new Pawn(other.name, other.MaxHp);
		clone.CurrentHp = other.CurrentHp;
		clone.id = other.id;
		clone.isDead = other.isDead;
		clone.knownSpells = new List<string>(other.knownSpells.Count);
		foreach(string spellId in other.knownSpells) {
			clone.knownSpells.Add(spellId);
		}
		clone.ailments = new Dictionary<string, int>(other.ailments);
		clone.equipped = new HashSet<string>(other.equipped);
		clone.SpellHealBonus = Attribute.Clone(other.SpellHealBonus);
		return clone;
	}
}
