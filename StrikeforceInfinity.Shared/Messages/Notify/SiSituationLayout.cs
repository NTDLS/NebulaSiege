﻿using NTDLS.StreamFraming.Payloads;
using StrikeforceInfinity.Shared.Payload;

namespace StrikeforceInfinity.Shared.Messages.Query
{
    /// <summary>
    /// Notification from the lobby owner containing the situation layout.
    /// This is then broadcast from the server to each connection.
    /// </summary>
    public class SiSituationLayout : IFramePayloadNotification
    {
        public List<SiSpriteLayout> SpriteLayouts { get; set; } = new();
    }
}
