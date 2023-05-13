/**
 * Example HERO application with Display Module on port8, and USB gamepad inserted into HERO.
 * Bar graphs and text will update when user changes button states and axis values on gamepad.
 * Tested with Phoenix Framework 5.6.0.0 (Phoenix NETMF: 5.1.5.0)
 */
/*
*  Software License Agreement
*
* Copyright (C) Cross The Road Electronics.  All rights
* reserved.00
* 
* Cross The Road Electronics (CTRE) licenses to you the right to 
* use, publish, and distribute copies of CRF (Cross The Road) firmware files (*.crf) and Software
* API Libraries ONLY when in use with Cross The Road Electronics hardware products.
* 
* THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
* WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
* LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
* PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
* CROSS THE ROAD ELECTRONICS BE LIABLE FOR ANY INCIDENTAL, SPECIAL, 
* INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
* PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
* BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
* THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
* SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
* (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE
*/
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using CTRE.Gadgeteer.Module;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix;
using Microsoft.SPOT.Hardware;

namespace DisplayModule_Example
{

    public class Program
    {
        static GameController _gamepad;
        static DisplayModule _displayModule;
        static Font _font;

        static DisplayModule.LabelSprite title;
        const int displayLines = 6;
        static DisplayModule.LabelSprite[] labels;

        static ChoiceMenu currMenu, type, testPWM, testCAN;
        static int prevButtons = 0;
        private const int BACK = (1 << 0);
        private const int ENTER = (1 << 1);
        private const int UP = (1 << 2);
        private const int DOWN = (1 << 3);


        public static void Main()
        {
            _gamepad = new GameController(UsbHostDevice.GetInstance());
            _displayModule = new DisplayModule(CTRE.HERO.IO.Port1, DisplayModule.OrientationType.Landscape);
            _font = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);

            title = _displayModule.AddLabelSprite(_font, DisplayModule.Color.White, 0, 0, 150, 16);
            labels = new DisplayModule.LabelSprite[displayLines + 1];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = _displayModule.AddLabelSprite(_font, DisplayModule.Color.Blue, 0, 16 * (i + 1), 150, 16);
            }

            type = new ChoiceMenu("What to test:", null);
            testPWM = new ChoiceMenu("Testing PWM:", type);
            testCAN = new ChoiceMenu("Testing CAN:", type);

            type.add("PWM_SERVO", testPWM);
            type.add("CAN_TALON", testCAN);
            type.add("EXIT");

            testPWM.add("SET_POS");
            testPWM.add("SWEEP");
            testPWM.add("STICK_CONTROL");

            testCAN.add("SET_POWER");
            testCAN.add("SET_POS");
            // PID?
            testCAN.add("STICK_CONTROL");

            currMenu = type;

            bool done = false;
            do
            {
                currMenu.displayChoices(title, labels);
                runMenus();
                done = runTests();
            } while (!done);
        }

        public static void runMenus()
        {
            bool done = false;
            while(!done)
            {
                int currButtons = getButtons();
                int change = currButtons ^ prevButtons;
                if (change != 0)
                {
                    int pressed = currButtons & change;
                    if ((pressed & BACK) != 0 && currMenu.getParentMenu() != null)
                    {
                        clearDisplay();
                        currMenu = currMenu.getParentMenu();
                    }
                    else if ((pressed & ENTER) != 0)
                    {
                        clearDisplay();
                        if (currMenu.getChildMenu() == null)
                        {
                            done = true;
                            return;
                        }
                        else
                        {
                            currMenu = currMenu.getChildMenu();
                        }
                    }
                    else if ((pressed & UP) != 0)
                    {
                        currMenu.up();
                    }
                    else if ((pressed & DOWN) != 0)
                    {
                        currMenu.down();
                    }
                    prevButtons = currButtons;
                    currMenu.displayChoices(title, labels);
                }
                Thread.Sleep(10);
            }
        }

        public static bool runTests()
        {
            switch (type.getChoiceText())
            {
                case "PWM_SERVO":
                    uint period = 20000;  // total interval (us)
                    uint duration = 1000; // pulse length (us)
                    PWM pwm_8 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, period, duration, PWM.ScaleFactor.Microseconds, false);
                    title.SetText("Resetting position...");
                    pwm_8.Start();
                    Thread.Sleep(1000); // Ensuring zero
                    switch (testPWM.getChoiceText())
                    {
                        case "SET_POS":
                            uint pos = 1000;
                            uint step = 20;
                            ChoiceMenu setServoPos = new ChoiceMenu("Set Position", testPWM);
                            setServoPos.add("Position: " + (pos - 1000));       // 0
                            setServoPos.add("Step: " + step);                   // 1
                            setServoPos.add("EXIT");                            // 2
                            setServoPos.displayChoices(title, labels);
                            bool done = false;
                            while(!done)
                            {
                                int currButtons = getButtons();
                                int change = currButtons ^ prevButtons;
                                if (change != 0)
                                {
                                    int pressed = currButtons & change;
                                    if ((pressed & UP) != 0)
                                    {
                                        setServoPos.up();
                                    }
                                    else if ((pressed & DOWN) != 0)
                                    {
                                        setServoPos.down();
                                    }
                                    else
                                    {
                                        switch (setServoPos.selected)
                                        {
                                            case 0:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    pos = (pos - step >= 1000) ? pos - step : 1000;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    pos = (pos + step <= 2000) ? pos + step : 2000;
                                                }
                                                setServoPos.getChoice().text = "Position: " + (pos - 1000);
                                                break;
                                            case 1:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    step = (step - 1 >= 0) ? step - 1 : 0;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    step = step + 1;
                                                }
                                                setServoPos.getChoice().text = "Step: " + step;
                                                break;
                                            case 2:
                                                if (((pressed & BACK) != 0) || ((pressed & ENTER) != 0))
                                                {
                                                    done = true;
                                                }
                                                break;
                                        }
                                    }
                                    pwm_8.Duration = pos;
                                    prevButtons = currButtons;
                                    setServoPos.displayChoices(title, labels);
                                }
                            }
                            break;
                        case "SWEEP":
                            title.SetText("Running Servo's range...");
                            int dir = 1;
                            int delta = 0;
                            do
                            {
                                pwm_8.Duration = duration + (uint)delta;
                                if (delta > 1000 || delta < 0)
                                {
                                    dir = dir * -1;
                                }
                                delta = delta + (dir * 5);
                                Thread.Sleep(10);
                            } while (!_gamepad.GetButton(1));
                            title.SetText("Exiting...");
                            Thread.Sleep(500);
                            break;
                        case "STICK_CONTROL":
                            title.SetText("Use Left Y");
                            do
                            {
                                labels[0].SetText("Position:" + (pwm_8.Duration - 1000));
                                pwm_8.Duration = duration + (uint)((-_gamepad.GetAxis(1) + 1) / 2 * 1000);
                                Thread.Sleep(10);
                            } while (!_gamepad.GetButton(1));
                            title.SetText("Exiting...");
                            Thread.Sleep(500);
                            break;
                    }
                    pwm_8.Stop();
                    break;
                case "CAN_TALON":
                    CTRE.Phoenix.MotorControl.CAN.TalonSRX talon = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(0);
                    title.SetText("Zeroing position...");
                    talon.Set(CTRE.Phoenix.MotorControl.ControlMode.Position, 0.0);
                    Thread.Sleep(1000); // Ensuring zero
                    switch (testCAN.getChoiceText())
                    {
                        case "SET_POWER":
                            double power = 0.0;
                            double pwrStep = 0.1;
                            ChoiceMenu setTalonPower = new ChoiceMenu("Set Power", testCAN);
                            setTalonPower.add("Power: " + power);       // 0
                            setTalonPower.add("Step: " + pwrStep);         // 1
                            setTalonPower.add("EXIT");                  // 2
                            setTalonPower.displayChoices(title, labels);
                            bool done = false;
                            while (!done)
                            {
                                int currButtons = getButtons();
                                int change = currButtons ^ prevButtons;
                                if (change != 0)
                                {
                                    int pressed = currButtons & change;
                                    if ((pressed & UP) != 0)
                                    {
                                        setTalonPower.up();
                                    }
                                    else if ((pressed & DOWN) != 0)
                                    {
                                        setTalonPower.down();
                                    }
                                    else
                                    {
                                        switch (setTalonPower.selected)
                                        {
                                            case 0:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    power = (power - pwrStep >= 0) ? power - pwrStep: 0.0;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    power = (power + pwrStep <= 1.0) ? power + pwrStep: 1.0;
                                                }
                                                setTalonPower.getChoice().text = "Power: " + power;
                                                break;
                                            case 1:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    pwrStep = (pwrStep - 0.01 >= 0) ? pwrStep - 0.01 : 0;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    pwrStep = pwrStep + 0.01;
                                                }
                                                setTalonPower.getChoice().text = "Step: " + pwrStep;
                                                break;
                                            case 2:
                                                if (((pressed & BACK) != 0) || ((pressed & ENTER) != 0))
                                                {
                                                    done = true;
                                                }
                                                break;
                                        }
                                    }
                                    talon.Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, power);
                                    prevButtons = currButtons;
                                    setTalonPower.displayChoices(title, labels);
                                }
                            }
                            break;
                        case "SET_POS":
                            uint pos = 0;
                            uint posStep = 256;
                            ChoiceMenu setTalonPos = new ChoiceMenu("Set Position", testCAN);
                            setTalonPos.add("Position: " + pos);        // 0
                            setTalonPos.add("Step: " + posStep);           // 1
                            setTalonPos.add("EXIT");                    // 2
                            setTalonPos.displayChoices(title, labels);
                            done = false;
                            while (!done)
                            {
                                int currButtons = getButtons();
                                int change = currButtons ^ prevButtons;
                                if (change != 0)
                                {
                                    int pressed = currButtons & change;
                                    if ((pressed & UP) != 0)
                                    {
                                        setTalonPos.up();
                                    }
                                    else if ((pressed & DOWN) != 0)
                                    {
                                        setTalonPos.down();
                                    }
                                    else
                                    {
                                        switch (setTalonPos.selected)
                                        {
                                            case 0:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    pos = pos - posStep;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    pos = pos + posStep;
                                                }
                                                setTalonPos.getChoice().text = "Position: " + pos;
                                                break;
                                            case 1:
                                                if ((pressed & BACK) != 0)
                                                {
                                                    posStep = (posStep - 128 >= 0) ? posStep - 128 : 0;
                                                }
                                                else if ((pressed & ENTER) != 0)
                                                {
                                                    posStep = posStep + 128;
                                                }
                                                setTalonPos.getChoice().text = "Step: " + posStep;
                                                break;
                                            case 2:
                                                if (((pressed & BACK) != 0) || ((pressed & ENTER) != 0))
                                                {
                                                    done = true;
                                                }
                                                break;
                                        }
                                    }
                                    talon.Set(CTRE.Phoenix.MotorControl.ControlMode.Position, pos);
                                    prevButtons = currButtons;
                                    setTalonPos.displayChoices(title, labels);
                                }
                            }
                            break;
                        case "STICK_CONTROL":
                            title.SetText("Use Left Y");
                            do
                            {
                                labels[0].SetText("Power: " + talon.GetMotorOutputPercent());
                                labels[1].SetText("Position: " + talon.GetSelectedSensorPosition());
                                talon.Set(CTRE.Phoenix.MotorControl.ControlMode.PercentOutput, -_gamepad.GetAxis(1));
                                Thread.Sleep(10);
                            } while (!_gamepad.GetButton(1));
                            title.SetText("Exiting...");
                            Thread.Sleep(500);
                            break;
                    }
                    break;
                case "EXIT":
                    title.SetText("Bye!");
                    return true;
            }
            return false;
        }

        public static int getButtons()
        {
            int buttons = 0;
            if (_gamepad.GetButton(1)) buttons |= BACK;
            if (_gamepad.GetButton(2)) buttons |= DOWN;
            if (_gamepad.GetButton(3)) buttons |= ENTER;
            if (_gamepad.GetButton(4)) buttons |= UP;
            return buttons;
        }

        public static void clearDisplay()
        {
            title.SetText("");
            foreach(DisplayModule.LabelSprite sprite in labels)
            {
                sprite.SetText("");
            }
        }
    }
}
