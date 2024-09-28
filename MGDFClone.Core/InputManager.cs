using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Core {
    public class InputManager : GameComponent {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private Array _inputValues;
        private Array _mouseValues;

        public InputManager(Game game) : base(game) {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
            _inputValues = Enum.GetValues(typeof(eKeyboardInput));
            _mouseValues = Enum.GetValues(typeof(eMouseInput));
        }

        public override void Update(GameTime gameTime) {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            base.Update(gameTime);
        }

        public bool IsPressed(Keys keys) => _currentKeyboardState.IsKeyDown(keys);
        public bool IsPressed(eMouseInput mouseInput) => _isMousePressed(_currentMouseState, mouseInput);
        public bool IsHeld(Keys key) => _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        public bool IsHeld(eMouseInput input) => _isMousePressed(_currentMouseState, input) && _isMousePressed(_previousMouseState, input);
        public bool JustPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        public bool JustPressed(eMouseInput input) => _isMousePressed(_currentMouseState, input) && !_isMousePressed(_previousMouseState, input);
        public bool JustReleased(Keys key) => !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        public bool JustReleased(eMouseInput input) => !_isMousePressed(_currentMouseState, input) && _isMousePressed(_previousMouseState, input);
        public Vector2 GetMousePosition() => _currentMouseState.Position.ToVector2();
        public bool IsMouseMoved() => _currentMouseState.X != _previousMouseState.X || _currentMouseState.Y != _previousMouseState.Y;
        public int GetMouseScroll() => _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;

        private static bool _isMousePressed(MouseState state, eMouseInput input) {
            switch (input) {
                case eMouseInput.LeftButton:
                    return state.LeftButton == ButtonState.Pressed;
                case eMouseInput.MiddleButton:
                    return state.MiddleButton == ButtonState.Pressed;
                case eMouseInput.RightButton:
                    return state.RightButton == ButtonState.Pressed;
                default:
                    return false;
            }
        }
    }
}
