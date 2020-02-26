using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using static AttributeModifier;

public class Pawn {

	public EventBus.EvtPawnInteger OnTakeDamage = new EventBus.EvtPawnInteger();
	public EventBus.EvtPawnInteger OnHeal = new EventBus.EvtPawnInteger();
	public EventBus.EvtPawn OnSetupTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnBeginTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnEndTurn = new EventBus.EvtPawn();
	public EventBus.EvtPawn OnSpellsChange = new EventBus.EvtPawn();
	public EventBus.EvtPawnInteger OnItemEquipped = new EventBus.EvtPawnInteger();
	public EventBus.EvtPawnInteger OnItemUnequipped = new EventBus.EvtPawnInteger();
	public EventBus.EvtPawnInteger OnItemUsed = new EventBus.EvtPawnInteger();

	private int id;
	private string name;

	public int CurrentHp { get; private set; }
	public int MaxHp { get; private set; }

	private List<int> knownSpells = new List<int>();
	private bool isDead = false;

	private int[] ailments = new int[DB.Ailments.Length];

	private List<int> equipped = new List<int>();

	public Attribute MissChance = new Attribute().SetBaseValue(0);

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

	public void Equip(int eqId) {
		equipped.Add(eqId);
		DB.Equipments[eqId].OnEquipped(this);
		OnItemEquipped.Invoke(null, this, eqId);
	}

	public void Unequip(Equipment eq) {
		Unequip(eq.GetId());
	}

	public bool HasEquipped(Equipment eq) {
		return HasEquipped(eq.GetId());
	}

	public bool HasEquipped(int eqId) {
		return equipped.Contains(eqId);
	}

	public void Unequip(int eqId) {
		equipped.Remove(eqId);
		DB.Equipments[eqId].OnUnequipped(this);
		OnItemUnequipped.Invoke(null, this, eqId);
	}

	public int[] GetEquipment() {
		return equipped.ToArray();
	}

	public void ApplyAilment(int ailmentId, int intensity) {
		ApplyAilment(DB.Ailments[ailmentId], intensity);
	}

	public void ApplyAilment(Ailment ailment, int intensity) {
		ailment.ApplyToPawn(this, intensity);
	}

	public void CmdApplyAilment(int ailmentId, int intensity) {
		CmdApplyAilment(DB.Ailments[ailmentId], intensity);
	}

	public void CmdApplyAilment(Ailment ailment, int intensity) {
		ApplyAilment(ailment, intensity);
		NetworkServer.SendToAll(GameMsg.UpdateAilment, new GameMsg.MsgIntegerArray(1, id, ailment.GetId(), intensity));
	}

	public void SetAilment(int ailmentId, int intensity) {
		int oldIntensity = ailments[ailmentId];
		ailments[ailmentId] = intensity;
		DB.Ailments[ailmentId].OnIntensityChange(this, oldIntensity, intensity);
	}

	public void SetAilment(Ailment ailment, int intensity) {
		SetAilment(ailment.GetId(), intensity);
	}

	public void CmdSetAilment(int ailmentId, int intensity) {
		SetAilment(ailmentId, intensity);
		NetworkServer.SendToAll(GameMsg.UpdateAilment, new GameMsg.MsgIntegerArray(0, id, ailmentId, intensity));
	}

	public void CmdSetAilment(Ailment ailment, int intensity) {
		CmdSetAilment(ailment.GetId(), intensity);
	}

	public int GetAilment(int ailmentId) {
		return ailments[ailmentId];
	}

	public int GetAilment(Ailment ailment) {
		return GetAilment(ailment.GetId());
	}

	public bool HasAilment(int ailmentId) {
		return ailments[ailmentId] > 0;
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

	public void CmdDamage(int dmg) {
		Damage(dmg);
		NetworkServer.SendToAll(GameMsg.TakeDamage, new GameMsg.MsgIntegerArray(id, dmg));
		//NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void CmdHeal(int heal) {
		Heal(heal);
		NetworkServer.SendToAll(GameMsg.Heal, new GameMsg.MsgIntegerArray(id, heal));
		//NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void Restore() {
		if(isDead) {
			CurrentHp = MaxHp / 8;
			isDead = false;
		} else {
			Heal(MaxHp / 8);
		}
		NetworkServer.SendToAll(GameMsg.UpdatePawn, new GameMsg.MsgPawn() { pawn = this });
	}

	public void Die() {
		isDead = true;
	}

	public bool IsAlive() {
		return !isDead;
	}

	public string GetName() {
		return name;
	}

	public void AddSpell(int spellId) {
		if(!knownSpells.Contains(spellId)) {
			knownSpells.Add(spellId);
		}
	}

	public void AddSpell(Spell spell) {
		AddSpell(spell.GetId());
	}

	public void RemoveSpell(int spellId) {
		if(knownSpells.Contains(spellId)) {
			knownSpells.Remove(spellId);
		}
	}

	public Spell[] GetSpells() {
		return knownSpells.Select((id) => DB.SpellList[id]).ToArray();
	}

	public List<int> GetKnownSpellIds() {
		return new List<int>(knownSpells);
	}

	public bool DoesKnowSpell(Spell spell) {
		return DoesKnowSpell(spell.GetId());
	}

	public bool DoesKnowSpell(int spellId) {
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
		for(int i = 0; i < ailments.Length; i++) {
			writer.Write(ailments[i]);
		}
		writer.Write(equipped.Count);
		foreach(int eqId in equipped) {
			writer.Write(eqId);
		}
	}

	public virtual void Deserialize(NetworkReader reader) {
		id = reader.ReadInt32();
		name = reader.ReadString();
		CurrentHp = reader.ReadInt32();
		MaxHp = reader.ReadInt32();
		isDead = reader.ReadBoolean();
		int knownSpellCount = reader.ReadInt32();
		knownSpells = new List<int>(knownSpellCount);
		for(int i = 0; i < knownSpellCount; i++) {
			knownSpells.Add(reader.ReadInt32());
		}
		for(int i = 0; i < ailments.Length; i++) {
			SetAilment(i, reader.ReadInt32());
		}
		equipped.Clear();
		int eqCount = reader.ReadInt32();
		for(int i = 0; i < eqCount; i++) {
			equipped.Add(reader.ReadInt32());
		}
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
		/*Pawn pawn = new Pawn(lobbyPlayer.charName, 16);
		pawn.id = lobbyPlayer.id;
		pawn.knownSpells.Add(0);
		pawn.knownSpells.Add(1);
		pawn.knownSpells.Add(2);
		pawn.knownSpells.Add(3);
		pawn.knownSpells.Add(4 + lobbyPlayer.id);
		pawn.knownSpells.Add(11);*/
		//for(int i = 0; i < 3; i++) {
		//	pawn.knownSpells.Add(UnityEngine.Random.Range(0, DB.SpellList.Length));
		//}
		//return pawn;
	}

	public static Pawn Clone(Pawn other) {
		Pawn clone = new Pawn(other.name, other.MaxHp);
		clone.CurrentHp = other.CurrentHp;
		clone.id = other.id;
		clone.isDead = other.isDead;
		clone.knownSpells = new List<int>(other.knownSpells.Count);
		foreach(int spellId in other.knownSpells) {
			clone.knownSpells.Add(spellId);
		}
		clone.ailments = other.ailments;
		clone.equipped = new List<int>(other.equipped.Count);
		foreach(int eqId in other.equipped) {
			clone.equipped.Add(eqId);
		}
		return clone;
	}
}
