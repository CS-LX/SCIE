﻿using Engine;
using GameEntitySystem;
using System;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class ComponentTrain : Component, IUpdateable
	{
		static readonly Vector3 center = new Vector3(0.5f, 0, 0.5f);
		static readonly Quaternion[] directions = new[]
		{
			Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0),
			Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.PI * 0.5f),
			Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.PI),
			Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.PI * 1.5f)
		};

		static readonly Quaternion upwardDirection = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathUtils.PI * 0.25f);
		static readonly Quaternion downwardDirection = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathUtils.PI * -0.25f);
		static readonly Vector3[] forwardVectors = new[]
		{
			new Vector3(0, 0, -1),
			new Vector3(-1, 0, 0),
			new Vector3(0, 0, 1),
			new Vector3(1, 0, 0)
		};
		
		internal ComponentBody m_componentBody;
		public ComponentTrain ParentBody;
		protected ComponentDamage componentDamage;
		protected float m_outOfMountTime;
		ComponentMount m_componentMount;
		public ComponentEngine ComponentEngine;
		int m_forwardDirection;
		Quaternion rotation;
		Vector3 forwardVector;

		public int Direction
		{
			get { return m_forwardDirection; }
			set
			{
				forwardVector = forwardVectors[value];
				m_forwardDirection = value;
				rotation = directions[value];
			}
		}

		public UpdateOrder UpdateOrder => 0;

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_componentBody = Entity.FindComponent<ComponentBody>(true);
			componentDamage = Entity.FindComponent<ComponentDamage>();
			m_componentMount = Entity.FindComponent<ComponentMount>(true);
			if ((ComponentEngine = Entity.FindComponent<ComponentEngine>()) != null)
				m_componentBody.CollidedWithBody += CollidedWithBody;
			else
				ParentBody = valuesDictionary.GetValue("ParentBody", default(EntityReference)).GetComponent<ComponentTrain>(Entity, idToEntityMap, false);
			
			Direction = valuesDictionary.GetValue("Direction", 0);
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			if (m_forwardDirection != 0)
				valuesDictionary.SetValue("Direction", m_forwardDirection);
			var value = EntityReference.FromId(ParentBody, entityToIdMap);
			if (!value.IsNullOrEmpty())
				valuesDictionary.SetValue("ParentBody", value);
		}

		public void CollidedWithBody(ComponentBody body)
		{
			if (body.Density - 4.76f <= float.Epsilon)
				return;
			Vector2 v = m_componentBody.Velocity.XZ - body.Velocity.XZ;
			float amount = v.LengthSquared() * .3f;
			if (amount < .02f) return;
			var health = body.Entity.FindComponent<ComponentHealth>();
			if (health != null)
				health.Injure(amount / health.AttackResilience, null, false, "Train");
			else
				body.Entity.FindComponent<ComponentDamage>()?.Damage(amount);
			body.ApplyImpulse(Math.Clamp(1.25f * 6f * (float)Math.Pow(m_componentBody.Mass / body.Mass, 0.33f), 0f, 6f) * Vector3.Normalize(body.Position - m_componentBody.Position));
		}

		public ComponentTrain FindNearestTrain()
		{
			var bodies = new DynamicArray<ComponentBody>();
			Utils.SubsystemBodies.FindBodiesAroundPoint(m_componentBody.Position.XZ, 2f, bodies);
			float num = 0f;
			ComponentTrain result = null;
			foreach (ComponentTrain train in bodies.Select(GetRailEntity))
			{
				if (train == null || train.Entity == Entity) continue;
				float score = 0f;
				const float maxDistance = 4f;
				if (train.m_componentBody.Velocity.LengthSquared() < 1f && train.Direction == Direction)
				{
					var v = train.m_componentBody.Position + Vector3.Transform(train.m_componentMount.MountOffset, train.m_componentBody.Rotation) - m_componentBody.Position;
					if (v.LengthSquared() <= maxDistance)
						score = maxDistance - v.LengthSquared();
				}
				if (score > num)
				{
					num = score;
					result = train;
				}
			}
			return result;
		}

		public void SetDirection(int value)
		{
			Direction = value;
			m_componentBody.Rotation = rotation;
		}

		public void Update(float dt)
		{
			if (ComponentEngine != null)
				ComponentEngine.Coordinates = new Point3((int)m_componentBody.Position.X, (int)m_componentBody.Position.Y, (int)m_componentBody.Position.Z);
			if (m_componentMount.Rider != null)
			{
				var player = m_componentMount.Rider.Entity.FindComponent<ComponentPlayer>(true);
				player.ComponentLocomotion.LookOrder = player.ComponentInput.PlayerInput.Look;
			}

			ComponentTrain t = this;
			int level = 0;
			for (; t.ParentBody != null; level++) t = t.ParentBody;
			if (level > 0)
			{
				var body = t.m_componentBody;
				var pos = body.Position;
				var r = body.Rotation;
				Utils.SubsystemTime.QueueGameTimeDelayedExecution(Utils.SubsystemTime.GameTime + 0.23 * level, delegate
				{
					if (body.Velocity.XZ.LengthSquared() > 10f)
					{
						m_componentBody.Position = pos;
						m_componentBody.Rotation = r;
					}
				});
				m_outOfMountTime = Vector3.DistanceSquared(ParentBody.m_componentBody.Position, m_componentBody.Position) > 8f
					? m_outOfMountTime + dt
					: 0f;
				ComponentDamage ComponentDamage = ParentBody.Entity.FindComponent<ComponentDamage>();
				if (m_outOfMountTime > 1f || (componentDamage != null && componentDamage.Hitpoints <= .05f) || ComponentDamage != null && ComponentDamage.Hitpoints <= .05f)
					ParentBody = null;
				return;
			}

			switch (Direction)
			{
				case 0:
				case 2:
					m_componentBody.Position = new Vector3((float)Math.Floor(m_componentBody.Position.X) + 0.5f, m_componentBody.Position.Y, m_componentBody.Position.Z);
					break;
				case 1:
				case 3:
					m_componentBody.Position = new Vector3(m_componentBody.Position.X, m_componentBody.Position.Y, (float)Math.Floor(m_componentBody.Position.Z) + 0.5f);
					break;
			}
			
			if (ComponentEngine != null && ComponentEngine.HeatLevel >= 100f && m_componentBody.StandingOnValue.HasValue)
			{
				var result = Utils.SubsystemTerrain.Raycast(m_componentBody.Position, m_componentBody.Position + new Vector3(0, -3f, 0), false, true, null);

				if (result.HasValue && Terrain.ExtractContents(result.Value.Value) == RailBlock.Index && (dt *= SimulateRail(RailBlock.GetRailType(Terrain.ExtractData(result.Value.Value)))) > 0f)
					m_componentBody.m_velocity += dt * rotation.GetForwardVector();
			}
			m_componentBody.Rotation = Quaternion.Slerp(m_componentBody.Rotation, rotation, 0.15f);
		}

		float SimulateRail(int railType)
		{
			if (RailBlock.IsCorner(railType))
			{
				if (GetOffsetOnDirection(m_componentBody.Position, m_forwardDirection) > 0.5f)
					Turn(railType);
				return 50f;
			}
			if (RailBlock.IsDirectionX(railType) ^ !RailBlock.IsDirectionX(m_forwardDirection))
			{
				rotation = railType > 5
					? railType - 6 != Direction ? directions[Direction] * upwardDirection : directions[Direction] * downwardDirection
					: directions[Direction];
				return railType > 5 && railType - 6 != Direction ? 30f : 50f;
			}
			return 0f;
		}

		bool Turn(int turnType)
		{
			if (Direction == turnType)
			{
				Direction = (Direction - 1) & 3;
				m_componentBody.Velocity = (float)Math.Abs(m_componentBody.Velocity.X + m_componentBody.Velocity.Z) * forwardVector;
				m_componentBody.Position = Vector3.Floor(m_componentBody.Position) + center;
				return true;
			}
			if (((Direction - 1) & 3) == turnType)
			{
				Direction = (Direction + 1) & 3;
				m_componentBody.Velocity = (float)Math.Abs(m_componentBody.Velocity.X + m_componentBody.Velocity.Z) * forwardVector;
				m_componentBody.Position = Vector3.Floor(m_componentBody.Position) + center;
				return true;
			}
			return false;
		}

		static float GetOffsetOnDirection(Vector3 vec, int direction)
		{
			float offset = (direction & 1) == 0 ? vec.Z - (float)Math.Floor(vec.Z) : vec.X - (float)Math.Floor(vec.X);
			return (direction & 2) == 0 ? 1 - offset : offset;
		}

		public static ComponentTrain GetRailEntity(Component b)
		{
			return b.Entity.FindComponent<ComponentTrain>();
		}
	}
}