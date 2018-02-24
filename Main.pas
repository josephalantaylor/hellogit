unit Main;

{
  This file copyright (C) 1999, 2000 by Joseph Alan Taylor.

  This source code is original work and the intelectual
  property of Joseph Alan Taylor.  This source code is
  not to be distributed by any means without express
  written permission of Joseph Alan Taylor.
}

interface

uses
  Windows, Forms, Graphics, Classes, Controls, ExtCtrls, Dialogs, Splash;

type
  TMForm = class(TForm)
    AboutImage: TImage;
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
  public
    { Public declarations }
    FLastIdle: Cardinal;
    procedure OnIdle(Sender: TObject; var Done: Boolean);
  end;

var
  MForm: TMForm;

implementation

{$R *.DFM}

uses
  DDUtil, DirectDraw, Constants, GameManager, GraphicsManager, SoundManager, InputManager, TerrainManager,
  EnemyManager, PlayerManager;

procedure TMForm.FormCreate(Sender: TObject);
var
  SplashForm: TSplashForm;
begin
  SplashForm := TSplashForm.Create(Self);
  try
    SplashForm.Show;
    SplashForm.Refresh;
    // set the dementions of the form's client area
    ClientWidth := OriginalScreenWidth;
    ClientHeight := OriginalScreenHeight;
    // create the graphics manager
    gGraphicsMgr := TGraphicsManager.Create(Self, Handle);
    // create the sound manager
    gSoundMgr := TSoundManager.Create(Self, Handle);
    // create the input manager
    gInputMgr := TInputManager.Create(Self, Handle);
    // create the terrain manager
    gTerrainMgr := TTerrainManager.Create(Self);
    // create the enemy manager
    gEnemyMgr := TEnemyManager.Create(Self);
    // create the player manager
    gPlayerMgr := TPlayerManager.Create(Self);  
    // create the game manager
    gGameMgr := TGameManager.Create(Self, Handle);
    // start the loop which controls the game
    FLastIdle := 0;
    Application.OnIdle := OnIdle;
  finally
    SplashForm.Free;
  end;
end;

procedure TMForm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  gPlayerMgr.Free;
  gEnemyMgr.Free;
  gTerrainMgr.Free;
  gInputMgr.Free;
//  gSoundMgr.Free;
  gGraphicsMgr.Free;
  // free the game manager
  gGameMgr.Free;
end;

procedure TMForm.OnIdle(Sender: TObject; var Done: Boolean);
var
  TickCount: Cardinal;
begin
  // don't allow this method to be called more than once every 64 miliseconds (about
  // 16 times per second) -- this will control game speed on very fast computers
  TickCount := GetTickCount;
  if TickCount >= FLastIdle + MaxAnimationSpeed then
  begin
    FLastIdle := TickCount;
    case gGameMgr.GameState of
      gsPlaying: gGameMgr.MoveGame;
      gsPaused, gsSplash:
      case gGameMgr.TempGameState of
        gsNone: gGameMgr.PollForMenu;
        gsAbout: gGameMgr.PollForAbout;
        gsHelp: gGameMgr.PollForHelp;
        gsSetupKeyboard: gGameMgr.PollForKeyboard;
        gsSetupJoystick: gGameMgr.PollForJoystick;
        gsSetupDisplay: gGameMgr.PollForDisplay;
      end;
    end;
    gGameMgr.DrawGame(True);
  end;
  Done := Tag <> 0; // continue loop
  if Done then Close;
end;

end.
