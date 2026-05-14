-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: May 14, 2026 at 07:33 AM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `clothingstore`
--

DELIMITER $$
--
-- Procedures
--
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetCustomerOrders` (IN `cust_id` INT)   BEGIN
    SELECT o.OrderID, o.OrderDate, o.TotalAmount
    FROM Orders o
    WHERE o.CustomerID = cust_id;
END$$

--
-- Functions
--
CREATE DEFINER=`root`@`localhost` FUNCTION `GetTotalSpent` (`cust_id` INT) RETURNS DECIMAL(10,2) DETERMINISTIC BEGIN
    DECLARE total DECIMAL(10,2);
    
    SELECT SUM(TotalAmount) INTO total
    FROM Orders
    WHERE CustomerID = cust_id;
    
    RETURN IFNULL(total,0);
END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `activity_log`
--

CREATE TABLE `activity_log` (
  `LogID` int(11) NOT NULL,
  `ActionType` varchar(20) DEFAULT NULL,
  `TableName` varchar(50) DEFAULT NULL,
  `RecordID` int(11) DEFAULT NULL,
  `ActionTime` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `activity_log`
--

INSERT INTO `activity_log` (`LogID`, `ActionType`, `TableName`, `RecordID`, `ActionTime`) VALUES
(1, 'INSERT', 'orders', 11, '2026-05-13 03:18:06'),
(2, 'UPDATE', 'products', 6, '2026-05-13 03:18:06'),
(3, 'UPDATE', 'products', 10, '2026-05-13 03:18:06'),
(4, 'UPDATE', 'products', 2, '2026-05-13 03:22:34'),
(5, 'UPDATE', 'products', 1, '2026-05-13 03:23:04'),
(6, 'INSERT', 'orders', 12, '2026-05-14 03:13:13'),
(7, 'UPDATE', 'products', 6, '2026-05-14 03:13:13'),
(8, 'UPDATE', 'products', 1, '2026-05-14 03:13:13'),
(9, 'UPDATE', 'products', 2, '2026-05-14 03:13:13'),
(10, 'UPDATE', 'products', 4, '2026-05-14 03:13:42'),
(11, 'UPDATE', 'products', 2, '2026-05-14 03:14:40');

-- --------------------------------------------------------

--
-- Table structure for table `customers`
--

CREATE TABLE `customers` (
  `CustomerID` int(11) NOT NULL,
  `FirstName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `ContactNo` varchar(15) DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `customers`
--

INSERT INTO `customers` (`CustomerID`, `FirstName`, `LastName`, `ContactNo`, `Email`) VALUES
(1, 'Andrea', 'Bombales', '09171234567', 'andrea@email.com'),
(2, 'Juan', 'Dela Cruz', '09181234567', 'juan@email.com'),
(3, 'Kath', 'regala', '09191234567', 'kath@email.com'),
(4, 'Pedro', 'Reyes', '09201234567', 'pedro@email.com'),
(5, 'Pauleen', 'Lopez', '09211234567', 'Pau@email.com'),
(6, 'Luis', 'Garcia', '09221234567', 'luis@email.com'),
(7, 'Julie', 'Tan', '09231234567', 'Julie@email.com'),
(8, 'Ramon', 'Villanueva', '09241234567', 'ramon@email.com'),
(9, 'Mika', 'Borlasa', '09251234567', 'Mika@email.com'),
(10, 'Mark', 'Rivera', '09261234567', 'mark@email.com');

-- --------------------------------------------------------

--
-- Table structure for table `orderdetails`
--

CREATE TABLE `orderdetails` (
  `OrderDetailID` int(11) NOT NULL,
  `OrderID` int(11) DEFAULT NULL,
  `ProductID` int(11) DEFAULT NULL,
  `Quantity` int(11) DEFAULT NULL,
  `Subtotal` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `orderdetails`
--

INSERT INTO `orderdetails` (`OrderDetailID`, `OrderID`, `ProductID`, `Quantity`, `Subtotal`) VALUES
(1, 1, 1, 2, 598.00),
(2, 1, 8, 1, 599.00),
(3, 2, 2, 1, 799.00),
(4, 3, 3, 1, 1299.00),
(5, 4, 4, 1, 999.00),
(6, 5, 5, 1, 399.00),
(7, 6, 6, 1, 899.00),
(8, 7, 7, 1, 499.00),
(9, 8, 8, 1, 599.00),
(10, 9, 9, 1, 1099.00),
(11, 11, 6, 1, 899.00),
(12, 11, 10, 2, 1398.00),
(13, 12, 6, 1, 899.00),
(14, 12, 1, 1, 299.00),
(15, 12, 2, 1, 799.00);

--
-- Triggers `orderdetails`
--
DELIMITER $$
CREATE TRIGGER `trg_after_orderdetails_delete` AFTER DELETE ON `orderdetails` FOR EACH ROW BEGIN

    -- OLD.OrderDetailID refers to the record that was deleted

    INSERT INTO activity_log(ActionType, TableName, RecordID)
    VALUES ('DELETE', 'orderdetails', OLD.OrderDetailID);

END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `orders`
--

CREATE TABLE `orders` (
  `OrderID` int(11) NOT NULL,
  `CustomerID` int(11) DEFAULT NULL,
  `OrderDate` date DEFAULT NULL,
  `TotalAmount` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `orders`
--

INSERT INTO `orders` (`OrderID`, `CustomerID`, `OrderDate`, `TotalAmount`) VALUES
(1, 1, '2026-02-20', 1098.00),
(2, 2, '2026-02-21', 799.00),
(3, 3, '2026-02-22', 1299.00),
(4, 4, '2026-02-23', 999.00),
(5, 5, '2026-02-24', 399.00),
(6, 6, '2026-02-25', 899.00),
(7, 7, '2026-02-26', 499.00),
(8, 8, '2026-02-27', 599.00),
(9, 9, '2026-02-28', 1099.00),
(10, 10, '2026-03-01', 699.00),
(11, 2, '2026-05-13', 2297.00),
(12, 3, '2026-05-14', 1997.00);

--
-- Triggers `orders`
--
DELIMITER $$
CREATE TRIGGER `trg_after_order_insert` AFTER INSERT ON `orders` FOR EACH ROW BEGIN
    -- Insert a record into the activity_log table
    -- NEW.OrderID refers to the ID of the newly inserted order
    INSERT INTO activity_log(ActionType, TableName, RecordID)
    VALUES ('INSERT', 'orders', NEW.OrderID);

END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `ProductID` int(11) NOT NULL,
  `ProductName` varchar(50) DEFAULT NULL,
  `Category` varchar(30) DEFAULT NULL,
  `Size` varchar(10) DEFAULT NULL,
  `Price` decimal(10,2) DEFAULT NULL,
  `Stock` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`ProductID`, `ProductName`, `Category`, `Size`, `Price`, `Stock`) VALUES
(1, 'T-Shirt', 'Top', 'M', 299.00, 51),
(2, 'Jeans', 'Bottom', 'L', 799.00, 39),
(3, 'Jacket', 'Outerwear', 'XL', 1299.00, 20),
(4, 'Dress', 'Women', 'S', 999.00, 32),
(5, 'Shorts', 'Bottom', 'M', 399.00, 40),
(6, 'Hoodie', 'Outerwear', 'L', 899.00, 33),
(7, 'Blouse', 'Women', 'S', 499.00, 45),
(8, 'Polo Shirt', 'Top', 'M', 599.00, 50),
(9, 'Sweater', 'Outerwear', 'L', 1099.00, 15),
(10, 'Skirt', 'Women', 'S', 699.00, 28);

--
-- Triggers `products`
--
DELIMITER $$
CREATE TRIGGER `trg_after_product_update` AFTER UPDATE ON `products` FOR EACH ROW BEGIN

    -- NEW.ProductID refers to the product that was updated

    INSERT INTO activity_log(ActionType, TableName, RecordID)
    VALUES ('UPDATE', 'products', NEW.ProductID);

END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `returns`
--

CREATE TABLE `returns` (
  `ReturnID` int(11) NOT NULL,
  `OrderID` int(11) NOT NULL,
  `OrderDetailID` int(11) NOT NULL,
  `ProductID` int(11) NOT NULL,
  `QtyReturned` int(11) NOT NULL DEFAULT 1,
  `RefundAmount` decimal(10,2) NOT NULL DEFAULT 0.00,
  `Reason` varchar(300) DEFAULT NULL,
  `ReturnDate` datetime DEFAULT current_timestamp(),
  `ProcessedBy` varchar(80) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `returns`
--

INSERT INTO `returns` (`ReturnID`, `OrderID`, `OrderDetailID`, `ProductID`, `QtyReturned`, `RefundAmount`, `Reason`, `ReturnDate`, `ProcessedBy`) VALUES
(1, 1, 1, 1, 2, 598.00, 'doesnt fit', '2026-05-13 11:23:04', NULL),
(2, 12, 15, 2, 1, 799.00, 'doesnt fit', '2026-05-14 11:14:40', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `suppliers`
--

CREATE TABLE `suppliers` (
  `SupplierID` int(11) NOT NULL,
  `SupplierName` varchar(50) DEFAULT NULL,
  `ContactNo` varchar(15) DEFAULT NULL,
  `Address` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `suppliers`
--

INSERT INTO `suppliers` (`SupplierID`, `SupplierName`, `ContactNo`, `Address`) VALUES
(1, 'ABC Clothing', '09170000001', 'Manila'),
(2, 'Urban Wear', '09170000002', 'Makati'),
(3, 'Fashion Hub', '09170000003', 'Quezon City'),
(4, 'Style Co.', '09170000004', 'Cebu'),
(5, 'Trendsetters', '09170000005', 'Davao'),
(6, 'ClothLine', '09170000006', 'Pasig'),
(7, 'Metro Apparel', '09170000007', 'Taguig'),
(8, 'Prime Fashion', '09170000008', 'Pasay'),
(9, 'Elite Wear', '09170000009', 'Mandaluyong'),
(10, 'Street Style', '09170000010', 'Las Piñas');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `UserID` int(11) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(30) NOT NULL DEFAULT 'Staff',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`UserID`, `FullName`, `Username`, `Email`, `PasswordHash`, `Role`, `IsActive`, `CreatedAt`, `UpdatedAt`) VALUES
(1, 'System Administrator', 'admin', 'admin@example.com', 'c15e93e1fe620af6f1c576eb6cb72754b60c733aeefd396996db2424e77329f4', 'Administrator', 1, '2026-05-12 17:15:58', '2026-05-13 10:20:30'),
(2, 'josephjesusmary', '@jojemarlab', 'joje@email.com', '5d4bab166ead6d326c4cfca4bb70ca214427b1bc53e49dd858309418d9a0f434', 'Administrator', 1, '2026-05-13 10:12:02', '2026-05-13 10:21:33'),
(3, 'kim mingyu', '@kim.mingyu', 'Mingyu22@gmail.com', '2c5abdef2a0eacb49f5115991a9331e8f78c5a15f986426815fd9486b5230628', 'Cashier', 1, '2026-05-13 10:21:23', '2026-05-13 10:22:28');

-- --------------------------------------------------------

--
-- Stand-in structure for view `vw_customerorders`
-- (See below for the actual view)
--
CREATE TABLE `vw_customerorders` (
`FirstName` varchar(50)
,`LastName` varchar(50)
,`OrderID` int(11)
,`OrderDate` date
,`TotalAmount` decimal(10,2)
);

-- --------------------------------------------------------

--
-- Stand-in structure for view `vw_orderdetailsreport`
-- (See below for the actual view)
--
CREATE TABLE `vw_orderdetailsreport` (
`OrderID` int(11)
,`ProductName` varchar(50)
,`Quantity` int(11)
,`Subtotal` decimal(10,2)
);

-- --------------------------------------------------------

--
-- Stand-in structure for view `vw_productsales`
-- (See below for the actual view)
--
CREATE TABLE `vw_productsales` (
`ProductName` varchar(50)
,`TotalSold` decimal(32,0)
);

-- --------------------------------------------------------

--
-- Structure for view `vw_customerorders`
--
DROP TABLE IF EXISTS `vw_customerorders`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `vw_customerorders`  AS SELECT `c`.`FirstName` AS `FirstName`, `c`.`LastName` AS `LastName`, `o`.`OrderID` AS `OrderID`, `o`.`OrderDate` AS `OrderDate`, `o`.`TotalAmount` AS `TotalAmount` FROM (`customers` `c` join `orders` `o` on(`c`.`CustomerID` = `o`.`CustomerID`)) ;

-- --------------------------------------------------------

--
-- Structure for view `vw_orderdetailsreport`
--
DROP TABLE IF EXISTS `vw_orderdetailsreport`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `vw_orderdetailsreport`  AS SELECT `o`.`OrderID` AS `OrderID`, `p`.`ProductName` AS `ProductName`, `od`.`Quantity` AS `Quantity`, `od`.`Subtotal` AS `Subtotal` FROM ((`orders` `o` join `orderdetails` `od` on(`o`.`OrderID` = `od`.`OrderID`)) join `products` `p` on(`od`.`ProductID` = `p`.`ProductID`)) ;

-- --------------------------------------------------------

--
-- Structure for view `vw_productsales`
--
DROP TABLE IF EXISTS `vw_productsales`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `vw_productsales`  AS SELECT `p`.`ProductName` AS `ProductName`, sum(`od`.`Quantity`) AS `TotalSold` FROM (`products` `p` join `orderdetails` `od` on(`p`.`ProductID` = `od`.`ProductID`)) GROUP BY `p`.`ProductName` ;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `activity_log`
--
ALTER TABLE `activity_log`
  ADD PRIMARY KEY (`LogID`);

--
-- Indexes for table `customers`
--
ALTER TABLE `customers`
  ADD PRIMARY KEY (`CustomerID`);

--
-- Indexes for table `orderdetails`
--
ALTER TABLE `orderdetails`
  ADD PRIMARY KEY (`OrderDetailID`),
  ADD KEY `OrderID` (`OrderID`),
  ADD KEY `ProductID` (`ProductID`);

--
-- Indexes for table `orders`
--
ALTER TABLE `orders`
  ADD PRIMARY KEY (`OrderID`),
  ADD KEY `CustomerID` (`CustomerID`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`ProductID`);

--
-- Indexes for table `returns`
--
ALTER TABLE `returns`
  ADD PRIMARY KEY (`ReturnID`),
  ADD KEY `OrderID` (`OrderID`),
  ADD KEY `OrderDetailID` (`OrderDetailID`),
  ADD KEY `ProductID` (`ProductID`);

--
-- Indexes for table `suppliers`
--
ALTER TABLE `suppliers`
  ADD PRIMARY KEY (`SupplierID`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `Username` (`Username`),
  ADD UNIQUE KEY `Email` (`Email`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `activity_log`
--
ALTER TABLE `activity_log`
  MODIFY `LogID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT for table `customers`
--
ALTER TABLE `customers`
  MODIFY `CustomerID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `orderdetails`
--
ALTER TABLE `orderdetails`
  MODIFY `OrderDetailID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT for table `orders`
--
ALTER TABLE `orders`
  MODIFY `OrderID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `ProductID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `returns`
--
ALTER TABLE `returns`
  MODIFY `ReturnID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `suppliers`
--
ALTER TABLE `suppliers`
  MODIFY `SupplierID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `orderdetails`
--
ALTER TABLE `orderdetails`
  ADD CONSTRAINT `orderdetails_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`),
  ADD CONSTRAINT `orderdetails_ibfk_2` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`);

--
-- Constraints for table `orders`
--
ALTER TABLE `orders`
  ADD CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customers` (`CustomerID`);

--
-- Constraints for table `returns`
--
ALTER TABLE `returns`
  ADD CONSTRAINT `returns_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `orders` (`OrderID`),
  ADD CONSTRAINT `returns_ibfk_2` FOREIGN KEY (`OrderDetailID`) REFERENCES `orderdetails` (`OrderDetailID`),
  ADD CONSTRAINT `returns_ibfk_3` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
