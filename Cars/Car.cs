// https://kenney.nl/assets/racing-pack
// https://kidscancode.org/godot_recipes/2d/car_steering/

using Godot;
using System;

public class Car : KinematicBody2D
{
    [Export]
    private float _wheelBase = 70;
    [Export]
    private int _steeringAngle = 5;
    [Export]
    private float _steerAngle;
    [Export]
    private float _enginePower = 800f;
    [Export]
    private float _friction = -0.9f;
    [Export]
    private float _drag = -0.0015f;
    [Export]
    private float _breaking = -450;
    [Export]
    private float _maxSpeedReverse = 250;
    [Export]
    private float _slipSpeed = 400;      // Speed where traction is reduced



    private Vector2 _velocity;
    private Vector2 _acceleration = Vector2.Zero;
    private float _tractionFast = 0.1f;  // High-speed traction
    private float _tractionSlow = 0.7f;  //Low-speed traction



    public override void _PhysicsProcess(float delta) {
        _acceleration = Vector2.Zero;
        GetInput();
        CalculateSteerAngle(delta);
        _velocity += _acceleration * delta;
        _velocity = MoveAndSlide(_velocity);
    }



    private void GetInput() {
        float turn = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        _steerAngle = turn * Mathf.Deg2Rad(_steeringAngle);

        if(Input.IsActionPressed("backward"))
            _acceleration = Transform.x * _breaking;

        if(Input.IsActionPressed("forward"))
            _acceleration = Transform.x * _enginePower;        

    }



    private void CalculateSteerAngle(float delta) {
        Vector2 rearWheel = Position - Transform.x * _wheelBase / 2.0f;
        Vector2 frontWheel = Position + Transform.x * _wheelBase / 2.0f;
        rearWheel += _velocity * delta;
        frontWheel += _velocity.Rotated(_steerAngle) * delta;
        Vector2 newHeading = (frontWheel - rearWheel).Normalized();

        float traction = _tractionSlow;
        if(_velocity.Length() > _slipSpeed)
            traction = _tractionFast;


        float d = newHeading.Dot(_velocity.Normalized());
        if(d < 0)
            _velocity = -newHeading * Mathf.Min(_velocity.Length(), _maxSpeedReverse);
        else if(d > 0)
            _velocity = _velocity.LinearInterpolate(newHeading * _velocity.Length(), traction);

        Rotation = newHeading.Angle();
    }



    private void ApplyFriction() {
        if(_velocity.Length() < 5)
            _velocity = Vector2.Zero;
        
        Vector2 frictionForce = _velocity * _friction;
        Vector2 dragForce = _velocity * _velocity.Length() * _drag;

        if(_velocity.Length() < 100)
            frictionForce *=3;

        _acceleration += dragForce + frictionForce;
    }
} 