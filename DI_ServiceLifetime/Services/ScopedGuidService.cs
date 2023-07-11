﻿namespace DI_ServiceLifetime.Services
{
    public class ScopedGuidService:IScopedGuidService
    {
        private readonly Guid Id;
        public ScopedGuidService()
        {
                Id = Guid.NewGuid();
        }
        public string GetGuid()
        {
            return Id.ToString();
        }
    }
}
