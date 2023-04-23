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
using Microsoft.SPOT;
using CTRE.Gadgeteer.Module;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix;

namespace DisplayModule_Example
{
    public class Program
    {
        GameController _gamepad = new GameController(UsbHostDevice.GetInstance());

        DisplayModule _displayModule = new DisplayModule(CTRE.HERO.IO.Port1, DisplayModule.OrientationType.Landscape);
        Font _font = Properties.Resources.GetFont(Properties.Resources.FontResources.NinaB);

        DisplayModule.LabelSprite _gamepadCxnLabel, leftXLabel, leftYLabel, rightXLabel, rightYLabel;

        public void RunForever()
        {
            _gamepadCxnLabel = _displayModule.AddLabelSprite(_font, DisplayModule.Color.White, 0, 0, 80, 16);
            leftXLabel = _displayModule.AddLabelSprite(_font, DisplayModule.Color.Blue, 10, 32, 100, 16);
            leftYLabel = _displayModule.AddLabelSprite(_font, DisplayModule.Color.Blue, 10, 48, 100, 16);
            rightXLabel = _displayModule.AddLabelSprite(_font, DisplayModule.Color.Blue, 10, 64, 100, 16);
            rightYLabel = _displayModule.AddLabelSprite(_font, DisplayModule.Color.Blue, 10, 80, 100, 16);

            while (true)
			{
                if (_gamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    _gamepadCxnLabel.SetText("Connected");
                    _gamepadCxnLabel.SetColor(DisplayModule.Color.Green);
                    leftXLabel.SetText("LeftX:"+_gamepad.GetAxis(0));
                    leftYLabel.SetText("LeftY" + _gamepad.GetAxis(1));
                    rightXLabel.SetText("RightX" + _gamepad.GetAxis(2));
                    rightYLabel.SetText("RightY" + _gamepad.GetAxis(5));

                }
                else
                {
                    _gamepadCxnLabel.SetText("No Gamepad");
                    _gamepadCxnLabel.SetColor(DisplayModule.Color.Red);
                }
                Thread.Sleep(10);
            }
        }
        public static void Main()
        {
            new Program().RunForever();
        }
    }
}
