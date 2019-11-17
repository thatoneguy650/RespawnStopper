using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class RespawnStopper : Script
{
    public bool Enabled { get; set; } = true;
    private bool DiedInVehicle = false;
    private bool isDead = false;
    private Keys RespawnInPlaceKey = Keys.L;
    private Keys RegularRespawnKey = Keys.R;
    private bool GameLoaded = false;
    private int TicksElapsed = 0;
    private TimeSpan TimeOfDeath;
    public RespawnStopper()
    {
        this.Tick += onTick;
        this.KeyDown += onKeyDown;
    }
    private void onTick(object sender, EventArgs e)
    {
        if (!GameLoaded && TicksElapsed++ == 100) { UI.Notify("RespawnStopper v0.1 By Greskrendtregk Loaded"); GameLoaded = true; }

        if (Enabled)
        {
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);
            Function.Call(Hash.SET_FADE_OUT_AFTER_ARREST, false);
            Function.Call(Hash.IGNORE_NEXT_RESTART, true);
            Function.Call(Hash._DISABLE_AUTOMATIC_RESPAWN, true);
            Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "respawn_controller");
        }
        else
        {
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, true);
            Function.Call(Hash.SET_FADE_OUT_AFTER_ARREST, true);
            Function.Call(Hash.IGNORE_NEXT_RESTART, false);
            Function.Call(Hash._DISABLE_AUTOMATIC_RESPAWN, false);
            Function.Call(Hash.REQUEST_SCRIPT, "respawn_controller");
        }

        StateTick();
    }
    private void onKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == RespawnInPlaceKey && isDead)
            RespawnInPlace();

        if (e.KeyCode == RegularRespawnKey && isDead)
            RespawnNormally();

    }
    private void StateTick()
    {
        if (Game.Player.Character.IsDead && !isDead)
        {
            isDead = true;
            TimeOfDeath = World.CurrentDayTime;
            DiedInVehicle = Game.Player.Character.IsInVehicle();
            Game.Player.WantedLevel = 0;
            Game.Player.Character.Kill();
            Game.Player.Character.Health = 0;
            Game.Player.Character.IsInvincible = true;     
            Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
        }
        Function.Call(Hash.DISPLAY_HUD, true);
    }
    private void RespawnInPlace()
    {
        try
        {
            Game.Player.Character.Health = 100;
            if (DiedInVehicle)
            {
                Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, Game.Player.Character);
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, Game.Player.Character.Position.X + 10F, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, GameplayCamera.Direction.ToHeading(), false, false);
                Function.Call(Hash._RESET_LOCALPLAYER_STATE);
                if (Game.Player.Character.LastVehicle.Exists() && Game.Player.Character.LastVehicle.IsDriveable)
                {
                    Game.Player.Character.SetIntoVehicle(Game.Player.Character.LastVehicle, VehicleSeat.Driver);
                }
                Function.Call(Hash._RESET_LOCALPLAYER_STATE);
            }
            else
            {
                Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, Game.Player.Character);
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, GameplayCamera.Direction.ToHeading(), false, false);
                Function.Call(Hash._RESET_LOCALPLAYER_STATE);
            }
            World.CurrentDayTime = TimeOfDeath;
            Game.TimeScale = 1f;
            DiedInVehicle = false;
            Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
            isDead = false;

        }
        catch (Exception e)
        {
            UI.Notify(e.Message);
        }
    }
    private void RespawnNormally()
    {
        Enabled = false;
        Wait(15000);
        while (Game.IsLoading)
            Yield();
        isDead = false;
        Enabled = true;
    }




}

