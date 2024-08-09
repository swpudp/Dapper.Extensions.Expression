CREATE DATABASE  if not exists `dapper_exp`;

use `dapper_exp`;

CREATE TABLE `attachment` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `Extend` varchar(45) NOT NULL,
  `Version` int(11) NOT NULL,
  PRIMARY KEY (`Id`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `buyer` (
  `Id` char(36) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `Type` tinyint(1) NOT NULL,
  `Code` varchar(45) NOT NULL,
  `Identity` varchar(45) DEFAULT NULL,
  `Email` varchar(45) DEFAULT NULL,
  `Mobile` varchar(45) DEFAULT NULL,
  `IsDelete` bit(1) NOT NULL,
  `IsActive` bit(1) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `Version` int(11) NOT NULL,
  PRIMARY KEY (`Id`,`CreateTime`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `items` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Index` int(11) NOT NULL,
  `Code` varchar(45) NOT NULL,
  `Name` varchar(45) NOT NULL,
  `Price` float(16,4) NOT NULL,
  `Quantity` float(16,4) NOT NULL,
  `Discount` float(16,4) NOT NULL,
  `Amount` float(16,4) NOT NULL,
  `Unit` varchar(45) DEFAULT NULL,
  `Version` int(11) NOT NULL,
  PRIMARY KEY (`Id`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `order` (
  `Id` char(36) NOT NULL,
  `BuyerId` char(36) NOT NULL,
  `Number` varchar(45) NOT NULL,
  `Remark` varchar(100) NOT NULL,
  `Status` tinyint(1) NOT NULL,
  `SignState` tinyint(1) DEFAULT NULL,
  `Amount` float(16,4) NOT NULL,
  `Freight` float(16,4) DEFAULT NULL,
  `DocId` char(36) DEFAULT NULL,
  `IsDelete` bit(1) NOT NULL,
  `IsActive` bit(2) not NULL default 0,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `Index` INT(11) NOT NULL DEFAULT 0,
  `Version` int(11) NOT NULL,
  PRIMARY KEY (`Id`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `naming_policy_snake_case` (
  `id` char(36) NOT NULL,
  `naming_type` tinyint NOT NULL,
  `create_time` datetime NOT NULL,
  `version` int(11) NOT NULL,
  PRIMARY KEY (`Id`,`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;