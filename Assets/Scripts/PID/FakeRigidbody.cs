using System;
using BML.Scripts.Attributes;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace BML.Scripts.PID
{
    public class FakeRigidbody : MonoBehaviour
    {
        #region Properties (Inspector)
        
        public float mass;
        public float drag;
        public float angularDrag;
        public float maxVelocity;
        public float maxAngularVelocity;
        
        public FakeRigidbodyConstraints constraints;
        
        // public Collider collider;
        
        public enum DeltaTimeMode
        {
            Unscaled,
            Scaled,
        }
        
        public DeltaTimeMode deltaTimeMode = DeltaTimeMode.Unscaled;
        private float DeltaTime => deltaTimeMode == DeltaTimeMode.Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

        #endregion

        #region State

        // The attached unity transform component will be used to represent the position and rotation of the fake rigidbody
        public Vector3 velocity;
        public Vector3 angularVelocity;

        #endregion
        
        private const ForceMode DefaultForceMode = ForceMode.Force;

        private float GetConstraintMaskComponent(FakeRigidbodyConstraints constraint)
        {
            return constraints.HasFlag(constraint) ? 0f : 1f;
        }

        public void AddForce(Vector3 force, ForceMode forceMode = DefaultForceMode)
        {
            if (constraints.HasFlag(FakeRigidbodyConstraints.FreezePosition))
            {
                return;
            }
            
            switch (forceMode)
            {
                case ForceMode.Force:
                    velocity += force / mass * DeltaTime;
                    break;
                case ForceMode.Acceleration:
                    velocity += force * DeltaTime;
                    break;
                case ForceMode.Impulse:
                    velocity += force / mass;
                    break;
                case ForceMode.VelocityChange:
                    velocity += force;
                    break;
            }
            
            velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
            
            var constraintMask = new Vector3(
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionX),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionY),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionZ)
            );
            velocity = Vector3.Scale(velocity, constraintMask);
        }
        
        public void AddRelativeForce(Vector3 force, ForceMode forceMode = DefaultForceMode)
        {
            AddForce(transform.TransformDirection(force), forceMode);
        }
        
        public void AddTorque(Vector3 torque, ForceMode forceMode = DefaultForceMode)
        {
            if (constraints.HasFlag(FakeRigidbodyConstraints.FreezeRotation))
            {
                return;
            }
            
            switch (forceMode)
            {
                case ForceMode.Force:
                    angularVelocity += torque / mass * DeltaTime;
                    break;
                case ForceMode.Acceleration:
                    angularVelocity += torque * DeltaTime;
                    break;
                case ForceMode.Impulse:
                    angularVelocity += torque / mass;
                    break;
                case ForceMode.VelocityChange:
                    angularVelocity += torque;
                    break;
            }
            
            angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxAngularVelocity);
            
            var constraintMask = new Vector3(
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationX),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationY),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationZ)
            );
            angularVelocity = Vector3.Scale(angularVelocity, constraintMask);
        }
        
        public void AddRelativeTorque(Vector3 torque, ForceMode forceMode = DefaultForceMode)
        {
            AddTorque(transform.TransformDirection(torque), forceMode);
        }

        private void UpdateFakeRigidbody()
        {
            var velocityConstraintMask = new Vector3(
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionX),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionY),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezePositionZ)
            );
            velocity = Vector3.Scale(velocity, velocityConstraintMask);
            
            var angularVelocityConstraintMask = new Vector3(
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationX),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationY),
                GetConstraintMaskComponent(FakeRigidbodyConstraints.FreezeRotationZ)
            );
            angularVelocity = Vector3.Scale(angularVelocity, angularVelocityConstraintMask);
            
            // apply drag
            velocity -= velocity * drag * DeltaTime;
            angularVelocity -= angularVelocity * angularDrag * DeltaTime;
            // apply velocity
            transform.position += velocity * DeltaTime;
            transform.rotation *= Quaternion.Euler(angularVelocity * DeltaTime);
        }
        
        #region Unity lifecycle
        
        private void Update()
        {
            if (deltaTimeMode == DeltaTimeMode.Unscaled)
            {
                UpdateFakeRigidbody();
            }
        }
        
        private void FixedUpdate()
        {
            if (deltaTimeMode == DeltaTimeMode.Scaled)
            {
                UpdateFakeRigidbody();
            }
        }

        #endregion
    }
}