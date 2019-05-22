﻿using System;
using DNTFrameworkCore.Domain.Entities;
using DNTFrameworkCore.EntityFramework.Context.Hooks;
using DNTFrameworkCore.Runtime;

namespace DNTFrameworkCore.EntityFramework.Context.Internal
{
    internal class RowLevelSecurityPreInsertHook : PreInsertHook<IHasRowLevelSecurity>
    {
        private readonly IUserSession _session;

        public RowLevelSecurityPreInsertHook(IUserSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override void Hook(IHasRowLevelSecurity entity, HookEntityMetadata metadata)
        {
            if (_session.UserId.HasValue) entity.UserId = _session.UserId.Value;
        }
    }
}