using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Mapping
{
    public class ModelMapper
    {
        private readonly IMapper mapper;
        public ModelMapper(IMapper mapper)
        {
            this.mapper = mapper;
        }
        public static MapperConfiguration Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
            });
            return config;
        }

        public static TDestination Map<TDestination>(object source) where TDestination : class
        {
            return Mapper.Map<TDestination>(source);
        }

        public static TDestination Map<TSource, TDestination>(TSource source) where TSource : class where TDestination : class
        {
            return Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts) where TDestination : class
        {
            return Mapper.Map<TDestination>(source, opts);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> opts) where TSource : class where TDestination : class
        {
            return Mapper.Map<TSource, TDestination>(source, opts);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination) where TSource : class where TDestination : class
        {
            return Mapper.Map(source, destination);
        }

        public static object Map(object source, Type sourceType, Type destinationType)
        {
            return Mapper.Map(source, sourceType, destinationType);
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> opts) where TSource : class where TDestination : class
        {
            return Mapper.Map(source, destination, opts);
        }

        public static object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Mapper.Map(source, destination, sourceType, destinationType);
        }

        public static object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return Mapper.Map(source, sourceType, destinationType, opts);
        }

        public static object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return Mapper.Map(source, destination, sourceType, destinationType, opts);
        }

        public static void Reset()
        {
            //TODO: Fix Reset
            //Mapper.Reset();
        }
    }
}
