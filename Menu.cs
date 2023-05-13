using Microsoft.SPOT;
using System;

namespace DisplayModule_Example
{
    public abstract class Menu
    {
        public abstract void displayMenu();
        public abstract Menu getChildMenu();
        public abstract void menuUp();
        public abstract void menuDown();

        public interface MenuButtons
        {
            Boolean upPressed();
            Boolean downPressed();
            Boolean enterPressed();
            Boolean backPressed();
        }

        private const long LOOP_INTERVAL = 20; //msec
        private const int MENUBUTTON_BACK = (1 << 0);
        private const int MENUBUTTON_ENTER = (1 << 1);
        private const int MENUBUTTON_UP = (1 << 2);
        private const int MENUBUTTON_DOWN = (1 << 3);
        private const int MENUBUTTON_ALT_UP = (1 << 4);
        private const int MENUBUTTON_ALT_DOWN = (1 << 5);

        private String menuTitle;
        private Menu parent;
        private MenuButtons menuButtons;

        private static int prevButtonStates = 0;
        private static Menu currMenu = null;

        protected Menu(String menuTitle, Menu parent, MenuButtons menuButtons)
        {
            if (menuTitle == null)
            {
                throw new ArgumentNullException("menuTitle cannot be null");
            }
            this.menuTitle = menuTitle;
            this.parent = parent;
            this.menuButtons = menuButtons;
        }

        public Menu getParentMenu()
        {
            return parent;
        }

        public String getTitle()
        {
            return menuTitle;
        }

        public static void setRootMenu(Menu rootMenu)
        {
            currMenu = rootMenu;
        }

        public static void walkMenuTree(Menu rootMenu)
        {
            setRootMenu(rootMenu);
            rootMenu.displayMenu();
        }

        public static Boolean runMenus()
        {
            Boolean done = false;

            if (currMenu == null)
            {
                done = true;
            }
            else
            {
                int currButtonStates = currMenu.getMenuButtons();
                int changedButtons = currButtonStates ^ prevButtonStates;
                if (changedButtons != 0)
                {
                    int buttonsPressed = currButtonStates & changedButtons;
                    if ((buttonsPressed & MENUBUTTON_BACK) != 0)
                    {
                        Menu parentMenu = currMenu.getParentMenu();
                        if (parentMenu != null)
                        {
                            currMenu = parentMenu;
                        }
                    }
                    else if ((buttonsPressed & MENUBUTTON_ENTER) != 0)
                    {
                        currMenu = currMenu.getChildMenu();
                    }
                    else if ((buttonsPressed & MENUBUTTON_UP) != 0)
                    {
                        currMenu.menuUp();
                    }
                    else if ((buttonsPressed & MENUBUTTON_DOWN) != 0)
                    {
                        currMenu.menuDown();
                    }
                    if (currMenu != null)
                    {
                        currMenu.displayMenu();
                    }
                    else
                    {
                        // Clear
                    }
                    prevButtonStates = currButtonStates;
                }
            }
            return done;
        }

        private int getMenuButtons()
        {
            int buttons = 0;
            if (menuButtons != null)
            {
                if (menuButtons.backPressed()) buttons |= MENUBUTTON_BACK;
                if (menuButtons.enterPressed()) buttons |= MENUBUTTON_ENTER;
                if (menuButtons.upPressed()) buttons |= MENUBUTTON_UP;
                if (menuButtons.downPressed()) buttons |= MENUBUTTON_DOWN;
            }
            else
            {
                // Default mapping
            }
            return buttons;
        }
    }
}
