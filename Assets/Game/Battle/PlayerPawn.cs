using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using static AttributeModifier;

public class PlayerPawn : Pawn {

	public Attribute[] Affinities = new Attribute[Element.Count];
	public Attribute DieCount = new Attribute().SetBaseValue(5);
	public Attribute LockCount = new Attribute();
	public Attribute RollCount = new Attribute().SetBaseValue(3);
	public Attribute SpellSlotCount = new Attribute().SetBaseValue(4);
	public Attribute EndOfBattleRestoration = new Attribute(new AttributeModifier("Base Value", Operation.MultiplyTotal, 0.125));

	public PlayerPawn(string name, int maxHp) : base(name, maxHp) {
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i] = new Attribute();
		}
	}

	public double GetAffinity(Element element) {
		return GetAffinity(element.GetId());
	}

	public double GetAffinity(int elemId) {
		return Affinities[elemId].GetValue(elemId == Element.Chaos.GetId() ? 0 : 10);
	}

	public double GetAffinityTotal() {
		double total = 0;	
		for(int i = 0; i < Affinities.Length; i++) {
			total += GetAffinity(i);
		}
		return total;
	}

	public override void Update(Pawn pawn) {
		base.Update(pawn);
		if(pawn is PlayerPawn) {
			PlayerPawn other = pawn as PlayerPawn;
			DieCount = Attribute.Clone(other.DieCount);
			EndOfBattleRestoration = Attribute.Clone(other.EndOfBattleRestoration);
		}
	}

	public override void Deserialize(NetworkReader reader) {
		base.Deserialize(reader);
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i].Deserialize(reader);
		}
		DieCount.Deserialize(reader);
		RollCount.Deserialize(reader);
		SpellSlotCount.Deserialize(reader);
		EndOfBattleRestoration.Deserialize(reader);
	}

	public override void Serialize(NetworkWriter writer) {
		base.Serialize(writer);
		for(int i = 0; i < Affinities.Length; i++) {
			Affinities[i].Serialize(writer);
		}
		DieCount.Serialize(writer);
		RollCount.Serialize(writer);
		SpellSlotCount.Serialize(writer);
		EndOfBattleRestoration.Serialize(writer);
	}
}
