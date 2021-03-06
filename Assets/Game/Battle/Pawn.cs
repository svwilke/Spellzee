﻿using System.Collections;
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

	public int CurrentHp { get; private set; }
	public int MaxHp { get; private set; }

	private List<string> knownSpells = new List<string>();
	private bool isDead = false;

	private Dictionary<string, int> ailments = new Dictionary<string, int>();

	private HashSet<string> equipped = new HashSet<string>();

	public Attribute MissChance = new Attribute().SetBaseValue(0);
	public Attribute SpellHealBonus = new Attribute();
	public Attribute SpellDamageBonus = new Attribute();

	public Attribute AilmentApplyBonus = new Attribute();
	public Attribute DamageReduction = new Attribute();

	public Pawn(string name, int maxHp) {
		this.name = name;
		this.MaxHp = CurrentHp = maxHp;
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

	public IEnumerable<KeyValuePair<string, int>> GetAilments() {
		return ailments;
	}

	public void ApplyAilment(string ailmentId, int intensity) {
		ApplyAilment(Ailments.Get(ailmentId), intensity);
	}

	public void ApplyAilment(Ailment ailment, int intensity) {
		ailment.ApplyToPawn(this, intensity);
	}

	public void CmdApplyAilment(string ailmentId, int intensity) {
		CmdApplyAilment(Ailments.Get(ailmentId), intensity);
	}

	public void CmdApplyAilment(Ailment ailment, int intensity) {
		ApplyAilment(ailment, intensity);
		NetworkServer.SendToAll(GameMsg.UpdateAilment, new GameMsg.MsgUpdateAilment() {
			updateType = GameMsg.MsgUpdateAilment.UpdateType.Apply,
			pawnId = id,
			ailmentId = ailment.GetId(),
			intensity = intensity
		});
	}

	public void SetAilment(string ailmentId, int intensity) {
		int oldIntensity = 0;
		if(ailments.ContainsKey(ailmentId)) {
			oldIntensity = ailments[ailmentId];
		}
		ailments[ailmentId] = intensity;
		Ailments.Get(ailmentId).OnIntensityChange(this, oldIntensity, intensity);
	}

	public void SetAilment(Ailment ailment, int intensity) {
		SetAilment(ailment.GetId(), intensity);
	}

	public void CmdSetAilment(string ailmentId, int intensity) {
		SetAilment(ailmentId, intensity);
		NetworkServer.SendToAll(GameMsg.UpdateAilment, new GameMsg.MsgUpdateAilment() {
			updateType = GameMsg.MsgUpdateAilment.UpdateType.Set,
			pawnId = id,
			ailmentId = ailmentId,
			intensity = intensity
		});
	}

	public void CmdSetAilment(Ailment ailment, int intensity) {
		CmdSetAilment(ailment.GetId(), intensity);
	}

	public int GetAilment(string ailmentId) {
		if(!ailments.ContainsKey(ailmentId)) {
			return 0;
		}
		return ailments[ailmentId];
	}

	public int GetAilment(Ailment ailment) {
		return GetAilment(ailment.GetId());
	}

	public bool HasAilment(string ailmentId) {
		return GetAilment(ailmentId) > 0;
	}

	public bool HasAilment(Ailment ailment) {
		return GetAilment(ailment.GetId()) > 0;
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

	/// <summary>
	/// Sets the fields of this Pawn to the same as those of the pawn given as an argument.
	/// </summary>
	/// <param name="pawn">The pawn with the fields to update to</param>
	public virtual void Update(Pawn pawn) {
		id = pawn.id;
		name = pawn.name;
		CurrentHp = pawn.CurrentHp;
		MaxHp = pawn.MaxHp;
		isDead = pawn.isDead;
		knownSpells = pawn.knownSpells;
		ailments = pawn.ailments;
		equipped = pawn.equipped;
	}

	public virtual void Serialize(NetworkWriter writer) {
		writer.Write(this is PlayerPawn);
		writer.Write(id);
		writer.Write(name);
		writer.Write(CurrentHp);
		writer.Write(MaxHp);
		writer.Write(isDead);
		writer.Write(knownSpells.Count);
		for(int i = 0; i < knownSpells.Count; i++) {
			writer.Write(knownSpells[i]);
		}
		writer.Write(ailments.Count);
		foreach(KeyValuePair<string, int> ailment in ailments) {
			writer.Write(ailment.Key);
			writer.Write(ailment.Value);
		}
		writer.Write(equipped.Count);
		foreach(string eqId in equipped) {
			writer.Write(eqId);
		}
		MissChance.Serialize(writer);
		SpellHealBonus.Serialize(writer);
	}

	public virtual void Deserialize(NetworkReader reader) {
		id = reader.ReadInt32();
		name = reader.ReadString();
		CurrentHp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
		isDead = reader.ReadBoolean();
		int knownSpellCount = reader.ReadInt32();
		knownSpells = new List<string>(knownSpellCount);
		for(int i = 0; i < knownSpellCount; i++) {
			AddSpell(reader.ReadString());
		}
		int ailmentCount = reader.ReadInt32();
		ailments = new Dictionary<string, int>(ailmentCount);
		for(int i = 0; i < ailmentCount; i++) {
			SetAilment(reader.ReadString(), reader.ReadInt32());
		}
		equipped.Clear();
		int eqCount = reader.ReadInt32();
		for(int i = 0; i < eqCount; i++) {
			Equip(reader.ReadString());
		}
		MissChance.Deserialize(reader);
		SpellHealBonus.Deserialize(reader);
	}

	public static Pawn DeserializeNew(NetworkReader reader) {
		Pawn p;
		if(reader.ReadBoolean()) {
			p = new PlayerPawn("", 1);
		} else {
			p = new Pawn("", 1);
		}
		p.Deserialize(reader);
		return p;
	}

	public static Pawn CreatePlayer(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		return DB.Classes[lobbyPlayer.charClass].Create(lobbyPlayer);
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
